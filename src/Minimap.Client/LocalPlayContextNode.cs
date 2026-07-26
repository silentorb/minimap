using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client;

/// <summary>Autoload: cross-scene local player roster and device bindings.</summary>
public partial class LocalPlayContextNode : Node
{
    public LocalPlayRoster Roster { get; } = new();

    public bool EnteredFromLobby { get; private set; }

    /// <summary>When true, lobby should hydrate from <see cref="Roster"/> instead of clearing.</summary>
    public bool ReturningFromSession { get; private set; }

    /// <summary>Profile id for the Achievements screen (set by Profiles before scene change).</summary>
    public Guid? ProfilesFocusId { get; set; }

    public string? ScenarioPath { get; set; }

    public void Clear()
    {
        Roster.Clear();
        EnteredFromLobby = false;
        ReturningFromSession = false;
        ProfilesFocusId = null;
        ScenarioPath = null;
    }

    public void ApplyDefaultSoloKeyboard()
    {
        EnteredFromLobby = false;
        ReturningFromSession = false;
        Roster.ApplyDefaultSoloKeyboard();
    }

    public void ApplyFromLobby(LocalPlayRoster roster)
    {
        ArgumentNullException.ThrowIfNull(roster);
        EnteredFromLobby = true;
        ReturningFromSession = false;
        Roster.CopyFrom(roster);
    }

    public void MarkReturningFromSession()
    {
        ReturningFromSession = true;
    }

    public void ClearReturningFromSession()
    {
        ReturningFromSession = false;
    }
}
