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
}
