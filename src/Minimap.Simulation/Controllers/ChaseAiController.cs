using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Chase nearest hostile with sticky lock; swing when in reach.</summary>
public sealed class ChaseAiController : IController
{
    private readonly Random _random;
    private IMoveSteering _steering;
    private float _reconsiderTimer;
    private int? _lockedTargetId;

    public ChaseAiController(Random random, IMoveSteering? steering = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _steering = steering ?? new DirectMoveSteering();
        _reconsiderTimer = 0f;
    }

    public Character? Pawn { get; private set; }

    public IMoveSteering Steering => _steering;

    public void Possess(Character character)
    {
        Pawn = character;
        var swing = Swing.FindSwingEffect(character);
        if (swing is not null)
            swing.CooldownRemaining = (float)(_random.NextDouble() * swing.FireIntervalSeconds);
        var shoot = Shoot.FindShootEffect(character);
        if (shoot is not null)
            shoot.CooldownRemaining = (float)(_random.NextDouble() * shoot.FireIntervalSeconds);
        _steering.ClearGoal();
        _reconsiderTimer = 0f;
        _lockedTargetId = null;
    }

    public void Unpossess()
    {
        _steering.ClearGoal();
        _steering.Dispose();
        _steering = new DirectMoveSteering();
        Pawn = null;
        _lockedTargetId = null;
    }

    public void ReplaceSteering(IMoveSteering steering)
    {
        ArgumentNullException.ThrowIfNull(steering);
        var previous = _steering;
        _steering = steering;
        previous.Dispose();
        _reconsiderTimer = 0f;
    }

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;

        _reconsiderTimer -= dt;
        if (_reconsiderTimer <= 0f || !IsLockedTargetValid(world))
            LockNearestHostile(world);

        var target = FindLockedTarget(world);
        if (target is not null)
            _steering.SetGoal(target.Position);
        else
            _steering.ClearGoal();

        Pawn.MoveIntent = _steering.SampleMoveIntent(Pawn, dt);

        var aimDir = SimVec2.Zero;
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

    private void LockNearestHostile(GameWorld world)
    {
        _reconsiderTimer = AiWanderGoals.NextChaseReconsiderDelay(_random);
        var nearest = Shoot.FindNearestHostile(Pawn!, world.Characters);
        _lockedTargetId = nearest?.Id;
    }

    private bool IsLockedTargetValid(GameWorld world)
    {
        if (_lockedTargetId is not int id)
            return false;
        foreach (var c in world.Characters)
        {
            if (c.Id != id)
                continue;
            return c.IsAlive && FactionRules.AreHostile(Pawn!.FactionId, c.FactionId);
        }

        return false;
    }

    private Character? FindLockedTarget(GameWorld world)
    {
        if (_lockedTargetId is not int id)
            return null;
        foreach (var c in world.Characters)
        {
            if (c.Id == id && c.IsAlive)
                return c;
        }

        return null;
    }
}
