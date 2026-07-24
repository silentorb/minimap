namespace Minimap.Simulation.Types;

/// <summary>Playthrough content bag. Not an integration-facing type.</summary>
public sealed class GameContent
{
    private readonly List<PlacedObjectDefinition> _placedObjects;
    private readonly List<ResourceDefinition> _resources;
    private readonly Dictionary<TagId, ResourceDefinition> _resourcesByTag = new();

    public GameContent(
        CharacterDefinition defaultCharacter,
        WeightedPool<SpawnerDefinition>? worldSpawnerPool = null,
        IEnumerable<PlacedObjectDefinition>? placedObjects = null,
        IEnumerable<ResourceDefinition>? resources = null)
    {
        ArgumentNullException.ThrowIfNull(defaultCharacter);
        DefaultCharacter = defaultCharacter;
        WorldSpawnerPool = worldSpawnerPool ?? WeightedPool<SpawnerDefinition>.Empty;
        _placedObjects = placedObjects?.ToList() ?? new List<PlacedObjectDefinition>();
        _resources = resources?.ToList() ?? new List<ResourceDefinition>();

        foreach (var resource in _resources)
        {
            ArgumentNullException.ThrowIfNull(resource);
            if (!_resourcesByTag.TryAdd(resource.Tag, resource))
            {
                throw new InvalidOperationException(
                    $"Duplicate resource tag for definition '{resource.Id}'.");
            }
        }

        HealthTag = RequireResourceTag(WellKnownResourceIds.Health);
        MaxHealthTag = RequireResourceTag(WellKnownResourceIds.MaxHealth);
    }

    public CharacterDefinition DefaultCharacter { get; }

    /// <summary>Weighted pool of spawner definitions placed during level init.</summary>
    public WeightedPool<SpawnerDefinition> WorldSpawnerPool { get; }

    public IReadOnlyList<PlacedObjectDefinition> PlacedObjects => _placedObjects;

    public IReadOnlyList<ResourceDefinition> Resources => _resources;

    public IReadOnlyDictionary<TagId, ResourceDefinition> ResourcesByTag => _resourcesByTag;

    public TagId HealthTag { get; }

    public TagId MaxHealthTag { get; }

    public bool TryGetResource(TagId tag, out ResourceDefinition? definition) =>
        _resourcesByTag.TryGetValue(tag, out definition);

    public bool TryGetResource(string id, out ResourceDefinition? definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(id))
            return false;

        foreach (var resource in _resources)
        {
            if (string.Equals(resource.Id, id, StringComparison.Ordinal))
            {
                definition = resource;
                return true;
            }
        }

        return false;
    }

    private TagId RequireResourceTag(string id)
    {
        if (!TryGetResource(id, out var definition) || definition is null)
        {
            throw new InvalidOperationException(
                $"Resource definition '{id}' is required in game content.");
        }

        return definition.Tag;
    }
}
