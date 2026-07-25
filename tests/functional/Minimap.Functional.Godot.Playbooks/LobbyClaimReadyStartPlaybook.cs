using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Keyboard claim + profile confirm + ready navigates to world with one human.</summary>
public sealed class LobbyClaimReadyStartPlaybook : IPlaybook
{
    public string Id => "LobbyClaimReadyStart";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        PlaybookProfileSeed.EnsureProfiles("Playbook");

        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);

        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "SelectingProfile")
        {
            return PlaybookResult.Fail(
                $"Slot 0 should be SelectingProfile; got {claimed.SlotModes.FirstOrDefault()}.");
        }

        // Confirm profile (Enter).
        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);

        var accessories = await context.GetLobbySnapshotAsync(cancellationToken);
        if (accessories.SlotModes.FirstOrDefault() != "SelectingAccessories")
        {
            return PlaybookResult.Fail(
                $"Slot 0 should be SelectingAccessories; got {accessories.SlotModes.FirstOrDefault()}.");
        }

        // Ready (Enter).
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
