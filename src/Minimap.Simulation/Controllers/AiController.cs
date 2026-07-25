using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>AI floor-goal wander + nearest-hostile shoot/swing aim (docs/game/features/gameplay/ai.md).</summary>
public sealed class AiController : IController
{
    private readonly Random _random;
    private IMoveSteering _steering;
    private float _retargetTimer;

    public AiController(Random random, IMoveSteering? steering = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _steering = steering ?? new DirectMoveSteering();
        _retargetTimer = 0f;
    }

    public Character? Pawn { get; private set; }

    public IMoveSteering Steering => _steering;

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
            PickNewWander(world);

        Pawn.MoveIntent = _steering.SampleMoveIntent(Pawn, dt);

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

    private void PickNewWander(GameWorld world)
    {
        _retargetTimer = AiWanderGoals.NextRetargetDelay(_random);
        var goal = AiWanderGoals.PickGrassGoalOrPause(world, _random);
        if (goal is null)
            _steering.ClearGoal();
        else
            _steering.SetGoal(goal.Value);
    }
}
