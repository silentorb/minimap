namespace Minimap.Simulation.Types;

/// <summary>Content definition for a static object placed on a map cell.</summary>
public sealed class PlacedObjectDefinition
{
    public PlacedObjectDefinition(
        string id,
        DepictionConfig? depictionConfig = null,
        string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Placed object definition id must be non-empty.", nameof(id));

        Id = id;
        DepictionConfig = depictionConfig;
        DisplayName = displayName;
    }

    public string Id { get; }

    public DepictionConfig? DepictionConfig { get; }

    public string? DisplayName { get; }
}
