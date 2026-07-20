using Minimap.Client.LocalPlay;

namespace Minimap.Client.Lobby;

/// <summary>Reconnect / drop eligibility when a joypad disconnects mid-game.</summary>
public sealed class ReconnectState
{
    public int? WaitingPlayerIndex { get; private set; }

    public bool IsWaiting => WaitingPlayerIndex is not null;

    public void BeginWait(int playerIndex) => WaitingPlayerIndex = playerIndex;

    public void ClearWait() => WaitingPlayerIndex = null;

    public bool CanRebindJoypad(LocalPlayRoster roster, int joypadDeviceIndex)
    {
        if (WaitingPlayerIndex is not int waiting)
            return false;
        return !roster.IsJoypadAssignedToOtherPlayer(joypadDeviceIndex, waiting);
    }

    public bool CanDropPlayer(LocalPlayRoster roster, InputDeviceId activator, Func<int, bool> isJoypadConnected)
    {
        if (WaitingPlayerIndex is not int waiting)
            return false;
        if (roster.FindPlayerIndexForDevice(activator) is not int activatorPlayer)
            return false;
        if (activatorPlayer == waiting)
            return false;
        return HasOtherConnectedPlayer(roster, waiting, isJoypadConnected);
    }

    public bool HasOtherConnectedPlayer(
        LocalPlayRoster roster,
        int exceptPlayerIndex,
        Func<int, bool> isJoypadConnected)
    {
        for (var i = 0; i < roster.PlayerCount; i++)
        {
            if (i == exceptPlayerIndex)
                continue;
            if (PlayerHasConnectedDevice(roster.Players[i], isJoypadConnected))
                return true;
        }

        return false;
    }

    private static bool PlayerHasConnectedDevice(
        LocalPlayerEntry player,
        Func<int, bool> isJoypadConnected)
    {
        foreach (var d in player.Devices)
        {
            if (d.IsKeyboard)
                return true;
            if (isJoypadConnected(d.JoypadDevice))
                return true;
        }

        return false;
    }
}
