namespace Minimap.Simulation.Types;

/// <summary>
/// Resource type lookup and well-known tags for character construction / clamping.
/// Built from <see cref="GameContent"/> or test catalogs.
/// </summary>
public sealed class ResourceContext
{
    private readonly Dictionary<TagId, ResourceDefinition> _byTag;

    public ResourceContext(
        IEnumerable<ResourceDefinition> resources,
        TagId healthTag,
        TagId maxHealthTag,
        TagId energyTag,
        TagId maxEnergyTag)
    {
        ArgumentNullException.ThrowIfNull(resources);
        _byTag = new Dictionary<TagId, ResourceDefinition>();
        foreach (var resource in resources)
        {
            ArgumentNullException.ThrowIfNull(resource);
            if (!_byTag.TryAdd(resource.Tag, resource))
            {
                throw new InvalidOperationException(
                    $"Duplicate resource tag for definition '{resource.Id}'.");
            }
        }

        HealthTag = healthTag;
        MaxHealthTag = maxHealthTag;
        EnergyTag = energyTag;
        MaxEnergyTag = maxEnergyTag;
        if (!_byTag.ContainsKey(HealthTag))
            throw new InvalidOperationException("Health resource type is not in the resource context.");
        if (!_byTag.ContainsKey(MaxHealthTag))
            throw new InvalidOperationException("Max health resource type is not in the resource context.");
        if (!_byTag.ContainsKey(EnergyTag))
            throw new InvalidOperationException("Energy resource type is not in the resource context.");
        if (!_byTag.ContainsKey(MaxEnergyTag))
            throw new InvalidOperationException("Max energy resource type is not in the resource context.");
    }

    public static ResourceContext FromGameContent(GameContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return new ResourceContext(
            content.Resources,
            content.HealthTag,
            content.MaxHealthTag,
            content.EnergyTag,
            content.MaxEnergyTag);
    }

    public TagId HealthTag { get; }

    public TagId MaxHealthTag { get; }

    public TagId EnergyTag { get; }

    public TagId MaxEnergyTag { get; }

    public IReadOnlyDictionary<TagId, ResourceDefinition> ByTag => _byTag;

    public bool TryGet(TagId tag, out ResourceDefinition? definition) =>
        _byTag.TryGetValue(tag, out definition);
}
