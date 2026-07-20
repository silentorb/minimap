using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client;

/// <summary>Autoload: cross-scene local player roster and device bindings.</summary>
public partial class LocalPlayContextNode : Node
{
    public LocalPlayRoster Roster { get; } = new();

    public bool EnteredFromLobby { get; private set; }

    public string? ScenarioPath { get; set; }

    public void Clear()
    {
        Roster.Clear();
        EnteredFromLobby = false;
        ScenarioPath = null;
    }

    public void ApplyDefaultSoloKeyboard()
    {
        EnteredFromLobby = false;
        Roster.ApplyDefaultSoloKeyboard();
    }

    public void ApplyFromLobby(LocalPlayRoster roster)
    {
        EnteredFromLobby = true;
        Roster.Clear();
        Roster.SetPlayerCount(roster.PlayerCount);
        for (var i = 0; i < roster.PlayerCount; i++)
        {
            foreach (var d in roster.Players[i].Devices)
                Roster.Players[i].AddDevice(d);
        }
    }
}
