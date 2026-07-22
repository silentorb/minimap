namespace Minimap.Simulation.Types;

/// <summary>Content definition for a map spawner and the characters it may emit.</summary>
public sealed class SpawnerDefinition
{
    public SpawnerDefinition(string id, WeightedPool<CharacterDefinition> characterPool)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Spawner definition id must be non-empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(characterPool);

        Id = id;
        CharacterPool = characterPool;
    }

    public string Id { get; }

    public WeightedPool<CharacterDefinition> CharacterPool { get; }
}
