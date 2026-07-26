using Minimap.Simulation.Types;

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

    /// <summary>Domain colors for the ability icon swatch (empty = baked black background).</summary>
    public IReadOnlyList<ColorRgb> DomainColors { get; init; } = Array.Empty<ColorRgb>();
}

/// <summary>Presentation snapshot for one player HUD slot (no simulation types).</summary>
public sealed class PlayerHudModel
{
    public required string DisplayName { get; init; }

    /// <summary>Absolute filesystem path to the profile avatar image, when set.</summary>
    public string? AvatarAbsolutePath { get; init; }

    public required IReadOnlyList<PlayerHudResourceModel> Resources { get; init; }
    public PlayerHudAbilityModel? SelectedAbility { get; init; }
}
