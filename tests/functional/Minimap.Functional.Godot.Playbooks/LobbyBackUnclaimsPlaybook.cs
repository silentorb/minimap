using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Back after claim returns slot to Available.</summary>
public sealed class LobbyBackUnclaimsPlaybook : IPlaybook
{
    public string Id => "LobbyBackUnclaims";

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

        await context.SetBackKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetBackKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        var snap = await context.GetLobbySnapshotAsync(cancellationToken);
        if (snap.SlotModes.FirstOrDefault() != "Available")
            return PlaybookResult.Fail($"Slot 0 should be Available; got {snap.SlotModes.FirstOrDefault()}.");

        return PlaybookResult.Success("back-unclaimed");
    }
}
