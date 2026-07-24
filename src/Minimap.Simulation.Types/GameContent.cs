namespace Minimap.Simulation.Types;

/// <summary>Playthrough content bag. Not an integration-facing type.</summary>
public sealed class GameContent
{
    private readonly List<PlacedObjectDefinition> _placedObjects;

    public GameContent(
        CharacterDefinition defaultCharacter,
        WeightedPool<SpawnerDefinition>? worldSpawnerPool = null,
        IEnumerable<PlacedObjectDefinition>? placedObjects = null)
    {
        ArgumentNullException.ThrowIfNull(defaultCharacter);
        DefaultCharacter = defaultCharacter;
        WorldSpawnerPool = worldSpawnerPool ?? WeightedPool<SpawnerDefinition>.Empty;
        _placedObjects = placedObjects?.ToList() ?? new List<PlacedObjectDefinition>();
    }

    public CharacterDefinition DefaultCharacter { get; }

    /// <summary>Weighted pool of spawner definitions placed during level init.</summary>
    public WeightedPool<SpawnerDefinition> WorldSpawnerPool { get; }

    public IReadOnlyList<PlacedObjectDefinition> PlacedObjects => _placedObjects;
}
