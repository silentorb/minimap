namespace Minimap.Simulation;

/// <summary>AI wander + nearest-hostile shoot aim (docs/game/features/ai.md).</summary>
public sealed class AiController : IController
{
    private readonly Random _random;
    private SimVec2 _walkDir;
    private float _retargetTimer;

    public AiController(Random random)
    {
        _random = random;
        PickNewWalk();
    }

    public Character? Pawn { get; private set; }

    public void Possess(Character character)
    {
        Pawn = character;
        var effect = Shoot.FindShootEffect(character);
        if (effect is not null)
            effect.CooldownRemaining = (float)(_random.NextDouble() * effect.FireIntervalSeconds);
        PickNewWalk();
    }

    public void Unpossess() => Pawn = null;

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;

        _retargetTimer -= dt;
        if (_retargetTimer <= 0f)
            PickNewWalk();

        Pawn.MoveIntent = _walkDir;

        var fireDir = SimVec2.Zero;
        var target = Shoot.FindNearestHostile(Pawn, world.Characters);
        if (target is not null)
        {
            var d = target.Position - Pawn.Position;
            if (d.LengthSquared >= 1e-10f)
                fireDir = d;
        }

        Shoot.Tick(world, Pawn, dt, fireDir);
    }

    private void PickNewWalk()
    {
        _retargetTimer = 0.6f + (float)_random.NextDouble() * 1.2f;
        // ~20% chance to pause briefly
        if (_random.NextDouble() < 0.2)
        {
            _walkDir = SimVec2.Zero;
            return;
        }

        var angle = (float)(_random.NextDouble() * Math.PI * 2.0);
        _walkDir = new SimVec2(MathF.Cos(angle), MathF.Sin(angle));
    }
}
