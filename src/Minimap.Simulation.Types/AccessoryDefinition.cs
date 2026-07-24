namespace Minimap.Simulation.Types;

/// <summary>Content definition for an accessory (effect templates only).</summary>
public sealed class AccessoryDefinition
{
    private readonly List<AccessoryEffect> _effectTemplates;
    private readonly List<TagId> _tags;

    public AccessoryDefinition(
        string id,
        IEnumerable<AccessoryEffect> effectTemplates,
        DepictionConfig? depictionConfig = null,
        IconConfig? iconConfig = null,
        IEnumerable<TagId>? tags = null,
        int pointCost = 0,
        string? displayName = null,
        string? description = null,
        AccessoryActivation? activation = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Accessory definition id must be non-empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(effectTemplates);
        if (pointCost < 0)
            throw new ArgumentOutOfRangeException(nameof(pointCost), "Point cost must be >= 0.");

        Id = id;
        _effectTemplates = effectTemplates.ToList();
        DepictionConfig = depictionConfig;
        IconConfig = iconConfig;
        _tags = tags?.ToList() ?? new List<TagId>();
        PointCost = pointCost;
        DisplayName = displayName;
        Description = description;
        Activation = activation ?? AccessoryActivation.None;
    }

    public string Id { get; }

    public IReadOnlyList<AccessoryEffect> EffectTemplates => _effectTemplates;

    public DepictionConfig? DepictionConfig { get; }

    public IconConfig? IconConfig { get; }

    public IReadOnlyList<TagId> Tags => _tags;

    public int PointCost { get; }

    public string? DisplayName { get; }

    public string? Description { get; }

    public AccessoryActivation Activation { get; }

    public bool HasTag(TagId tag) => _tags.Contains(tag);

    /// <summary>Create a runtime accessory with cloned effect instances.</summary>
    public Accessory CreateInstance()
    {
        var effects = _effectTemplates.Select(t => t.Clone()).ToList();
        return new Accessory(this, effects);
    }
}
