namespace Minimap.Simulation.Types;

/// <summary>Content definition for a map spawner and the actors it may emit.</summary>
public sealed class SpawnerDefinition
{
    public SpawnerDefinition(string id, WeightedPool<ActorDefinition> actorPool)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Spawner definition id must be non-empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(actorPool);

        Id = id;
        ActorPool = actorPool;
    }

    public string Id { get; }

    public WeightedPool<ActorDefinition> ActorPool { get; }
}
