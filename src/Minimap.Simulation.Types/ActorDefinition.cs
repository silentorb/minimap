namespace Minimap.Simulation.Types;

/// <summary>Content definition for a world actor (cell-anchored or character base).</summary>
public class ActorDefinition
{
    private readonly List<AccessoryDefinition> _accessories;

    public ActorDefinition(
        string id,
        IEnumerable<AccessoryDefinition>? accessories = null,
        DepictionConfig? depictionConfig = null,
        IconConfig? iconConfig = null,
        string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Actor definition id must be non-empty.", nameof(id));

        Id = id;
        _accessories = accessories?.ToList() ?? new List<AccessoryDefinition>();
        DepictionConfig = depictionConfig;
        IconConfig = iconConfig;
        DisplayName = displayName;
    }

    public string Id { get; }

    public IReadOnlyList<AccessoryDefinition> Accessories => _accessories;

    public DepictionConfig? DepictionConfig { get; }

    public IconConfig? IconConfig { get; }

    public string? DisplayName { get; }
}
