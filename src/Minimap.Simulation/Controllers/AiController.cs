namespace Minimap.Simulation;

/// <summary>AI floor-goal wander + nearest-hostile shoot aim (docs/game/features/ai.md).</summary>
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
        var effect = Shoot.FindShootEffect(character);
        if (effect is not null)
            effect.CooldownRemaining = (float)(_random.NextDouble() * effect.FireIntervalSeconds);
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

        var fireDir = SimVec2.Zero;
        var target = Shoot.FindNearestHostile(Pawn, world.Characters);
        if (target is not null)
        {
            var d = target.Position - Pawn.Position;
            if (d.LengthSquared >= 1e-10f)
                fireDir = d;
        }

        Shoot.Tick(world, Pawn, dt, fireDir, wantsFire: fireDir.LengthSquared >= 1e-10f);
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
