using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client;

/// <summary>Autoload: cross-scene local player roster and device bindings.</summary>
public partial class LocalPlayContextNode : Node
{
    public LocalPlayRoster Roster { get; } = new();

    public void Clear() => Roster.Clear();

    public void ApplyDefaultSoloKeyboard() => Roster.ApplyDefaultSoloKeyboard();

    public void ApplyFromLobby(LocalPlayRoster roster)
    {
        Roster.Clear();
        Roster.SetPlayerCount(roster.PlayerCount);
        for (var i = 0; i < roster.PlayerCount; i++)
        {
            foreach (var d in roster.Players[i].Devices)
                Roster.Players[i].AddDevice(d);
        }
    }
}
