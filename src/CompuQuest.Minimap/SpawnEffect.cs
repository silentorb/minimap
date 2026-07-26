using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Intrinsic spawner: emit characters from a weighted pool on an interval.</summary>
public sealed class SpawnEffect : AccessoryEffect, ISpawnEffect
{
    private readonly WeightedPool<string> _pool;
    private float _elapsed;

    public SpawnEffect(float intervalSeconds, int volume, WeightedPool<string> pool)
    {
        if (intervalSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
        if (volume < 1)
            throw new ArgumentOutOfRangeException(nameof(volume));
        ArgumentNullException.ThrowIfNull(pool);
        if (pool.IsEmpty)
            throw new ArgumentException("Spawn pool must be non-empty.", nameof(pool));

        IntervalSeconds = intervalSeconds;
        Volume = volume;
        _pool = pool;
    }

    public float IntervalSeconds { get; }

    public int Volume { get; }

    public void Tick(GameWorld world, Actor actor, float dt)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);
        if (dt <= 0f || !actor.IsAlive || actor.Cell is not HexAxial cell)
            return;

        _elapsed += dt;
        while (_elapsed >= IntervalSeconds)
        {
            _elapsed -= IntervalSeconds;
            for (var i = 0; i < Volume; i++)
            {
                if (!_pool.TryPick(world.Random, out var characterId) || string.IsNullOrWhiteSpace(characterId))
                    break;
                if (!world.TryGetCharacterDefinition(characterId, out var definition) || definition is null)
                    continue;

                var seekCrops = AiController.CharacterSeeksCrops(definition);
                world.TrySpawnNearbyHostile(
                    cell,
                    definition,
                    world.RivalFactionId,
                    AiTuning.DefaultAggression,
                    seekCrops);
            }
        }
    }

    public override AccessoryEffect Clone() =>
        new SpawnEffect(IntervalSeconds, Volume, _pool);
}
