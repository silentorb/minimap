namespace Minimap.Client;

/// <summary>Presentation snapshot for one player HUD slot (no simulation types).</summary>
public sealed class PlayerHudModel
{
    public required string DisplayName { get; init; }
    public float Health { get; init; }
    public float MaxHealth { get; init; }
}
