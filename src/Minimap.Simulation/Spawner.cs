using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Map marker that emits hostile AI during waves from a character pool.</summary>
public sealed class Spawner
{
    public Spawner(int id, HexAxial position, WeightedPool<CharacterDefinition> characterPool)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        ArgumentNullException.ThrowIfNull(characterPool);

        Id = id;
        Position = position;
        CharacterPool = characterPool;
    }

    public int Id { get; }

    public HexAxial Position { get; }

    public WeightedPool<CharacterDefinition> CharacterPool { get; }
}
