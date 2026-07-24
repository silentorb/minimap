namespace Minimap.Client;

/// <summary>One visible resource row for the player HUD (no simulation types).</summary>
public sealed class PlayerHudResourceModel
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public string? IconPath { get; init; }
    public int Amount { get; init; }
    public int? MaxAmount { get; init; }
}

/// <summary>Selected modal ability for the player HUD (no simulation types).</summary>
public sealed class PlayerHudAbilityModel
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public string? IconPath { get; init; }
}

/// <summary>Presentation snapshot for one player HUD slot (no simulation types).</summary>
public sealed class PlayerHudModel
{
    public required string DisplayName { get; init; }
    public required IReadOnlyList<PlayerHudResourceModel> Resources { get; init; }
    public PlayerHudAbilityModel? SelectedAbility { get; init; }
}
