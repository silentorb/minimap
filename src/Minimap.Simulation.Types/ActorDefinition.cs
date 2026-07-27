namespace Minimap.Simulation.Types;

/// <summary>Starting resource amount on an actor definition (applied at construction).</summary>
public readonly struct ActorResourceAmount
{
    public ActorResourceAmount(TagId tag, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        Tag = tag;
        Amount = amount;
    }

    public TagId Tag { get; }
    public int Amount { get; }
}

/// <summary>Content definition for a world actor (cell-anchored or character base).</summary>
public class ActorDefinition
{
    private readonly List<AccessoryDefinition> _accessories;
    private readonly List<ActorResourceAmount> _resources;

    public ActorDefinition(
        string id,
        IEnumerable<AccessoryDefinition>? accessories = null,
        DepictionConfig? depictionConfig = null,
        IconConfig? iconConfig = null,
        string? displayName = null,
        IEnumerable<ActorResourceAmount>? resources = null,
        float? size = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Actor definition id must be non-empty.", nameof(id));
        if (size is <= 0f)
            throw new ArgumentOutOfRangeException(nameof(size));

        Id = id;
        _accessories = accessories?.ToList() ?? new List<AccessoryDefinition>();
        DepictionConfig = depictionConfig;
        IconConfig = iconConfig;
        DisplayName = displayName;
        _resources = resources?.ToList() ?? new List<ActorResourceAmount>();
        Size = size;
    }

    public string Id { get; }

    public IReadOnlyList<AccessoryDefinition> Accessories => _accessories;

    public IReadOnlyList<ActorResourceAmount> Resources => _resources;

    public DepictionConfig? DepictionConfig { get; }

    public IconConfig? IconConfig { get; }

    public string? DisplayName { get; }

    /// <summary>Optional base collision radius (projectile actors).</summary>
    public float? Size { get; }
}
