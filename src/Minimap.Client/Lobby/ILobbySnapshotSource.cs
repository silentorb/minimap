using Minimap.Client.Lobby;

namespace Minimap.Client.Lobby;

/// <summary>Automation snapshot of lobby panel modes.</summary>
public sealed class LobbySnapshot
{
    public bool IsLobbyScene { get; init; }
    public IReadOnlyList<LobbySlotMode> SlotModes { get; init; } = Array.Empty<LobbySlotMode>();
    public int ClaimedCount { get; init; }
    public bool CanStartGame { get; init; }
}

/// <summary>Exposes lobby state to playbooks without scraping the scene tree.</summary>
public interface ILobbySnapshotSource
{
    LobbySnapshot GetLobbySnapshot();
}
