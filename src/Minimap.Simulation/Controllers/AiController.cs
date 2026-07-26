using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>
/// Aggression-blended wander + significant-goal pull; nearest-hostile combat;
/// optional crop harvest / eat (docs/game/features/gameplay/ai.md).
/// </summary>
public sealed class AiController : IController
{
    private readonly Random _random;
    private IMoveSteering _steering;
    private float _retargetTimer;
    private readonly float _aggression;
    private readonly bool _seekCrops;

    public AiController(
        Random random,
        IMoveSteering? steering = null,
        float aggression = AiTuning.DefaultAggression,
        bool seekCrops = false)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _steering = steering ?? new DirectMoveSteering();
        _aggression = Math.Clamp(aggression, 0f, 1f);
        _seekCrops = seekCrops;
        _retargetTimer = 0f;
    }

    public Character? Pawn { get; private set; }

    public IMoveSteering Steering => _steering;

    public float Aggression => _aggression;

    public bool SeekCrops => _seekCrops;

    public void Possess(Character character)
    {
        Pawn = character;
        var shoot = Shoot.FindShootEffect(character);
        if (shoot is not null)
            shoot.CooldownRemaining = (float)(_random.NextDouble() * shoot.FireIntervalSeconds);
        var swing = Swing.FindSwingEffect(character);
        if (swing is not null)
            swing.CooldownRemaining = (float)(_random.NextDouble() * swing.FireIntervalSeconds);
        _steering.ClearGoal();
        _retargetTimer = 0f;
    }

    public void Unpossess()
    {
        _steering.ClearGoal();
        _steering.Dispose();
        _steering = new DirectMoveSteering();
        Pawn = null;
    }

    /// <summary>Swap move steering (e.g. App upgrades Direct → Godot crowd). Disposes the previous steering.</summary>
    public void ReplaceSteering(IMoveSteering steering)
    {
        ArgumentNullException.ThrowIfNull(steering);
        var previous = _steering;
        _steering = steering;
        previous.Dispose();
        _retargetTimer = 0f;
    }

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;

        _retargetTimer -= dt;
        if (_retargetTimer <= 0f)
            PickGoal(world);

        Pawn.MoveIntent = _steering.SampleMoveIntent(Pawn, dt);

        if (_seekCrops)
            TryFarmerActions(world);

        var aimDir = SimVec2.Zero;
        var target = Shoot.FindNearestHostile(Pawn, world.Characters);
        if (target is not null)
        {
            var d = target.Position - Pawn.Position;
            if (d.LengthSquared >= 1e-10f)
                aimDir = d;
        }

        var hasAim = aimDir.LengthSquared >= 1e-10f;
        Shoot.Tick(world, Pawn, dt, aimDir, wantsFire: hasAim);

        var wantsSwing = false;
        if (hasAim && target is not null)
        {
            var swing = Swing.FindSwingEffect(Pawn);
            if (swing is not null)
            {
                var radius = swing.Radius > 0f ? swing.Radius : world.HexSize;
                var reach = radius + world.PlayerRadius;
                var distSq = (target.Position - Pawn.Position).LengthSquared;
                wantsSwing = distSq <= reach * reach;
            }
        }

        Swing.Tick(world, Pawn, dt, aimDir, wantsSwing);
    }

    private void PickGoal(GameWorld world)
    {
        _retargetTimer = AiWanderGoals.NextRetargetDelay(_random);
        var significant = FindSignificantGoal(world);
        var roam = AiWanderGoals.PickGrassGoalOrPause(world, _random);

        if (_aggression <= 0f || significant is null)
        {
            if (roam is null)
                _steering.ClearGoal();
            else
                _steering.SetGoal(roam.Value);
            return;
        }

        if (_aggression >= 1f)
        {
            _steering.SetGoal(significant.Value);
            return;
        }

        // Mid aggression: blend roam with a pull toward the significant target.
        // If paused (no roam), pull partway from current position toward the target.
        var from = roam ?? Pawn!.Position;
        var blended = Lerp(from, significant.Value, _aggression);
        _steering.SetGoal(blended);
    }

    private SimVec2? FindSignificantGoal(GameWorld world)
    {
        SimVec2? best = null;
        var bestDistSq = float.PositiveInfinity;
        var origin = Pawn!.Position;

        foreach (var other in world.Characters)
        {
            if (!other.IsAlive || other.Id == Pawn.Id)
                continue;
            if (!FactionRules.AreHostile(Pawn.FactionId, other.FactionId))
                continue;
            var distSq = (other.Position - origin).LengthSquared;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = other.Position;
            }
        }

        if (_seekCrops)
        {
            foreach (var actor in world.CellActors.Values)
            {
                if (!actor.IsAlive || actor.Cell is not HexAxial cell)
                    continue;
                var mature = false;
                foreach (var effect in actor.Effects)
                {
                    if (effect is IGrowEffect grow && grow.IsMature)
                    {
                        mature = true;
                        break;
                    }
                }

                if (!mature)
                    continue;

                var pos = HexWorldLayout.ToWorld(cell, world.HexSize);
                var distSq = (pos - origin).LengthSquared;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = pos;
                }
            }
        }

        return best;
    }

    private void TryFarmerActions(GameWorld world)
    {
        TryEat();
        TryHarvest(world);
    }

    private void TryEat()
    {
        if (Pawn!.MaxEnergy - Pawn.Energy < AiTuning.EatEnergyDeficitThreshold)
            return;
        if (!TrySelectModal(AiTuning.EatAccessoryId))
            return;
        AbilityLoadout.TryActivateInstantUse(Pawn.AbilityLoadout.SelectedModal, Pawn);
    }

    private void TryHarvest(GameWorld world)
    {
        var front = EnvironmentInteraction.ResolveTarget(world, Pawn!);
        if (front is null)
            return;

        var mature = false;
        foreach (var effect in front.Effects)
        {
            if (effect is IGrowEffect grow && grow.IsMature)
            {
                mature = true;
                break;
            }
        }

        if (!mature)
            return;

        if (!TrySelectModal(AiTuning.FarmAccessoryId))
            return;

        // Face the crop cell so interact resolution stays on the front hex.
        if (front.Cell is HexAxial cell)
        {
            var cropPos = HexWorldLayout.ToWorld(cell, world.HexSize);
            var dir = cropPos - Pawn!.Position;
            if (dir.LengthSquared >= 1e-10f)
                Pawn.Facing = dir.Normalized();
        }

        EnvironmentInteraction.TryInteract(world, Pawn!);
    }

    private bool TrySelectModal(string accessoryId)
    {
        var modals = Pawn!.AbilityLoadout.Modal;
        for (var i = 0; i < modals.Count; i++)
        {
            if (string.Equals(modals[i].Definition.Id, accessoryId, StringComparison.Ordinal))
            {
                Pawn.AbilityLoadout.SelectModal(i);
                return true;
            }
        }

        return false;
    }

    public static bool CharacterSeeksCrops(CharacterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        foreach (var accessory in definition.Accessories)
        {
            if (string.Equals(accessory.Id, AiTuning.FarmAccessoryId, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static SimVec2 Lerp(SimVec2 a, SimVec2 b, float t) =>
        a + (b - a) * t;
}
