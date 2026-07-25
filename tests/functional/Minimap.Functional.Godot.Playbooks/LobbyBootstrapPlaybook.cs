using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Lobby scene shows four Available panels.</summary>
public sealed class LobbyBootstrapPlaybook : IPlaybook
{
    public string Id => "LobbyBootstrap";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(15, cancellationToken);
        var snap = await context.GetLobbySnapshotAsync(cancellationToken);
        if (!snap.IsLobbyScene)
            return PlaybookResult.Fail("Expected lobby as main scene.");
        if (snap.SlotModes.Count != 4)
            return PlaybookResult.Fail($"Expected 4 slots; got {snap.SlotModes.Count}.");
        if (snap.SlotModes.Any(m => m != "Available"))
            return PlaybookResult.Fail($"Expected all Available; got {string.Join(',', snap.SlotModes)}.");

        return PlaybookResult.Success($"slots={string.Join(',', snap.SlotModes)}");
    }
}
