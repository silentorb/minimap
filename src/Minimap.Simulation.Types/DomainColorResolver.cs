namespace Minimap.Simulation.Types;

/// <summary>
/// Resolves domain background colors for an accessory from tags that match registered domains,
/// in domain registration order.
/// </summary>
public static class DomainColorResolver
{
    public static IReadOnlyList<ColorRgb> Resolve(
        AccessoryDefinition accessory,
        IReadOnlyList<DomainDefinition> domains)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        ArgumentNullException.ThrowIfNull(domains);

        if (domains.Count == 0 || accessory.Tags.Count == 0)
            return Array.Empty<ColorRgb>();

        var accessoryTags = new HashSet<TagId>(accessory.Tags);
        var colors = new List<ColorRgb>();
        foreach (var domain in domains)
        {
            if (accessoryTags.Contains(domain.Tag))
                colors.Add(domain.Color);
        }

        return colors;
    }
}
