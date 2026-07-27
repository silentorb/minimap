namespace Minimap.Client.Lobby;

/// <summary>Which wizard step nav buttons apply for a lobby slot mode.</summary>
public static class LobbyStepNavigation
{
    /// <summary>True when the mode has an earlier wizard step (or unclaim).</summary>
    public static bool CanGoBack(LobbySlotMode mode) =>
        mode is LobbySlotMode.SelectingProfile
            or LobbySlotMode.SelectingAccessories
            or LobbySlotMode.Ready;

    /// <summary>True when the mode has a later wizard step to advance into.</summary>
    public static bool CanGoForward(LobbySlotMode mode) =>
        mode is LobbySlotMode.SelectingProfile
            or LobbySlotMode.SelectingAccessories;

    /// <summary>
    /// True when Back/Forward may take GUI focus (pad/keyboard). False while SelectingAccessories
    /// so focus stays on the accessory grids; device B / Start still move steps.
    /// </summary>
    public static bool AllowsButtonFocus(LobbySlotMode mode) =>
        mode is LobbySlotMode.SelectingProfile or LobbySlotMode.Ready;
}
