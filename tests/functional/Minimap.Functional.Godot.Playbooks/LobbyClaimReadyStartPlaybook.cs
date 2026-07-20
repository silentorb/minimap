using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Keyboard claim + ready navigates to world with one human.</summary>
public sealed class LobbyClaimReadyStartPlaybook : IPlaybook
{
    public string Id => "LobbyClaimReadyStart";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);

        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "Claimed")
            return PlaybookResult.Fail($"Slot 0 should be Claimed; got {claimed.SlotModes.FirstOrDefault()}.");

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var world = await context.GetWorldSnapshotAsync(cancellationToken);
        if (!world.IsWorldRoot)
            return PlaybookResult.Fail("Expected world scene after lobby start.");
        if (world.HumanPlayerCount != 1)
            return PlaybookResult.Fail($"Expected 1 human; got {world.HumanPlayerCount}.");

        return PlaybookResult.Success($"humans={world.HumanPlayerCount}");
    }
}
