namespace Minimap.Simulation.Types;

/// <summary>Extension-registered thematic domain. Identity is <see cref="Tag"/> (id string → TagRegistry).</summary>
public sealed class DomainDefinition
{
    public DomainDefinition(
        string id,
        TagId tag,
        ColorRgb color,
        string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Domain definition id must be non-empty.", nameof(id));

        Id = id;
        Tag = tag;
        Color = color;
        DisplayName = displayName;
    }

    public string Id { get; }

    public TagId Tag { get; }

    public ColorRgb Color { get; }

    public string? DisplayName { get; }
}
