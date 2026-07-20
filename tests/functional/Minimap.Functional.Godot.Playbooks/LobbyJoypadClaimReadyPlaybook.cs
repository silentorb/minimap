using Godot;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Injected joypad A/Start claims and starts world.</summary>
public sealed class LobbyJoypadClaimReadyPlaybook : IPlaybook
{
    public string Id => "LobbyJoypadClaimReady";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await context.SetJoypadButtonAsync(0, (int)JoyButton.A, true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetJoypadButtonAsync(0, (int)JoyButton.A, false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);

        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "Claimed")
            return PlaybookResult.Fail($"Expected Claimed; got {claimed.SlotModes.FirstOrDefault()}.");

        await context.SetJoypadButtonAsync(0, (int)JoyButton.Start, true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetJoypadButtonAsync(0, (int)JoyButton.Start, false, cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var world = await context.GetWorldSnapshotAsync(cancellationToken);
        if (!world.IsWorldRoot)
            return PlaybookResult.Fail("Expected world after joypad ready.");

        return PlaybookResult.Success("joypad-started");
    }
}
