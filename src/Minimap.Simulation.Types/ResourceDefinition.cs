namespace Minimap.Simulation.Types;

/// <summary>Extension-registered resource type. Identity is <see cref="Tag"/> (id string → TagRegistry).</summary>
public sealed class ResourceDefinition
{
    public ResourceDefinition(
        string id,
        TagId tag,
        string? displayName = null,
        IconConfig? iconConfig = null,
        TagId? limitTag = null,
        bool visible = true,
        int uiPriority = 0)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Resource definition id must be non-empty.", nameof(id));

        Id = id;
        Tag = tag;
        DisplayName = displayName;
        IconConfig = iconConfig;
        LimitTag = limitTag;
        Visible = visible;
        UiPriority = uiPriority;
    }

    public string Id { get; }

    public TagId Tag { get; }

    public string? DisplayName { get; }

    public IconConfig? IconConfig { get; }

    /// <summary>When set, current amount is clamped to the amount of this limit resource.</summary>
    public TagId? LimitTag { get; }

    public bool Visible { get; }

    /// <summary>HUD sort key; higher first. May be negative.</summary>
    public int UiPriority { get; }
}
