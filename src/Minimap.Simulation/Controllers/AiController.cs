using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>
/// Aggression-blended wander + significant-goal pull; owner follow; injury flee;
/// nearest-hostile combat; optional crop harvest / eat (docs/game/features/gameplay/ai.md).
/// </summary>
public sealed class AiController : IController
{
    private readonly Random _random;
    private IMoveSteering _steering;
    private float _retargetTimer;
    private float _fleeRemaining;
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
        _fleeRemaining = 0f;
    }

    public Actor? Pawn { get; private set; }

    public IMoveSteering Steering => _steering;

    public float Aggression => _aggression;

    public bool SeekCrops => _seekCrops;

    /// <summary>Seconds remaining in an active injury flee; 0 when not fleeing.</summary>
    public float FleeRemaining => _fleeRemaining;

    public void Possess(Actor character)
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
        _fleeRemaining = 0f;
    }

    public void Unpossess()
    {
        _steering.ClearGoal();
        _steering.Dispose();
        _steering = new DirectMoveSteering();
        Pawn = null;
        _fleeRemaining = 0f;
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

        if (_fleeRemaining > 0f)
            _fleeRemaining = Math.Max(0f, _fleeRemaining - dt);

        _retargetTimer -= dt;
        if (_retargetTimer <= 0f)
            PickGoal(world);

        Pawn.MoveIntent = _steering.SampleMoveIntent(Pawn, dt);

        if (_seekCrops)
            TryFarmerActions(world);

        var aimDir = SimVec2.Zero;
        var target = Shoot.FindNearestHostile(Pawn, world.Actors);
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

        if (_fleeRemaining <= 0f && IsSeriouslyInjured(Pawn!) && TryStartFlee())
            _fleeRemaining = AiTuning.FleeDurationSeconds;

        if (_fleeRemaining > 0f)
        {
            SetFleeGoal(world);
            return;
        }

        var owner = FindLivingOwner(world);
        var significant = FindSignificantGoal(world);

        if (owner is not null)
        {
            PickOwnedGoal(world, owner, significant);
            return;
        }

        var roam = AiWanderGoals.PickGrassGoalOrPause(world, _random);
        ApplyAggressionBlend(roam, significant);
    }

    private void PickOwnedGoal(GameWorld world, Actor owner, SimVec2? significant)
    {
        var stayRadius = AiTuning.FollowStayRadiusHexes * world.HexSize;
        var distSq = (owner.Position - Pawn!.Position).LengthSquared;
        SimVec2? anchor = distSq <= stayRadius * stayRadius
            ? null
            : owner.Position;
        ApplyAggressionBlend(anchor, significant);
    }

    private void ApplyAggressionBlend(SimVec2? anchor, SimVec2? significant)
    {
        if (_aggression <= 0f || significant is null)
        {
            if (anchor is null)
                _steering.ClearGoal();
            else
                _steering.SetGoal(anchor.Value);
            return;
        }

        if (_aggression >= 1f)
        {
            _steering.SetGoal(significant.Value);
            return;
        }

        // Mid aggression: blend anchor with a pull toward the significant target.
        // If paused (no anchor), pull partway from current position toward the target.
        var from = anchor ?? Pawn!.Position;
        var blended = Lerp(from, significant.Value, _aggression);
        _steering.SetGoal(blended);
    }

    private void SetFleeGoal(GameWorld world)
    {
        var owner = FindLivingOwner(world);
        if (owner is not null)
        {
            _steering.SetGoal(owner.Position);
            return;
        }

        var hostile = Shoot.FindNearestHostile(Pawn!, world.Actors);
        if (hostile is null)
        {
            _steering.ClearGoal();
            return;
        }

        var away = Pawn!.Position - hostile.Position;
        if (away.LengthSquared < 1e-10f)
        {
            _steering.ClearGoal();
            return;
        }

        var escapeDistance = world.HexSize * 3f;
        _steering.SetGoal(Pawn.Position + away.Normalized() * escapeDistance);
    }

    private bool TryStartFlee() =>
        _random.NextDouble() < 1.0 - _aggression;

    private static bool IsSeriouslyInjured(Actor pawn)
    {
        if (!pawn.IsDestructible || pawn.MaxHealth <= 0)
            return false;
        return (float)pawn.Health / pawn.MaxHealth <= AiTuning.SeriousInjuryHealthFraction;
    }

    private Actor? FindLivingOwner(GameWorld world)
    {
        if (Pawn?.OwnerActorId is not int ownerId)
            return null;

        foreach (var other in world.Actors)
        {
            if (other.Id == ownerId && other.IsAlive)
                return other;
        }

        return null;
    }

    private SimVec2? FindSignificantGoal(GameWorld world)
    {
        SimVec2? best = null;
        var bestDistSq = float.PositiveInfinity;
        var origin = Pawn!.Position;

        foreach (var other in world.Actors)
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

    public static bool ActorSeeksCrops(ActorDefinition definition)
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
