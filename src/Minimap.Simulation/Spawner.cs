using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Map marker that emits hostile AI during waves from an actor pool.</summary>
public sealed class Spawner
{
    public Spawner(int id, HexAxial position, WeightedPool<ActorDefinition> actorPool)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(actorPool);

        Id = id;
        Position = position;
        ActorPool = actorPool;
    }

    public int Id { get; }

    public HexAxial Position { get; }

    public WeightedPool<ActorDefinition> ActorPool { get; }
}
