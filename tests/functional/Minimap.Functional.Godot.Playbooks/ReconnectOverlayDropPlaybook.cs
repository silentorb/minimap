using Godot;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Two-player start, simulated disconnect, keyboard player drops other.</summary>
public sealed class ReconnectOverlayDropPlaybook : IPlaybook
{
    public string Id => "ReconnectOverlayDrop";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        PlaybookProfileSeed.EnsureProfiles("PlaybookA", "PlaybookB");

        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        // Claim both players before advancing either (ready auto-starts when all claimed are ready).
        await TapActivate(context, cancellationToken);
        await TapJoy(context, 0, JoyButton.A, cancellationToken);

        // Confirm profiles, then ready.
        await TapActivate(context, cancellationToken);
        await TapJoy(context, 0, JoyButton.Start, cancellationToken);
        await TapActivate(context, cancellationToken);
        await TapJoy(context, 0, JoyButton.Start, cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var worldBefore = await context.GetWorldSnapshotAsync(cancellationToken);
        if (worldBefore.HumanPlayerCount != 2)
            return PlaybookResult.Fail($"Expected 2 humans; got {worldBefore.HumanPlayerCount}.");

        await context.SimulateJoypadDisconnectAsync(1, cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        var overlay = await context.GetPauseOverlaySnapshotAsync(cancellationToken);
        if (!overlay.Visible || !overlay.TreePaused)
            return PlaybookResult.Fail("Expected pause overlay after disconnect.");

        await TapActivate(context, cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        var overlayAfter = await context.GetPauseOverlaySnapshotAsync(cancellationToken);
        if (overlayAfter.Visible)
            return PlaybookResult.Fail("Overlay should hide after drop.");

        var worldAfter = await context.GetWorldSnapshotAsync(cancellationToken);
        if (worldAfter.HumanPlayerCount != 1)
            return PlaybookResult.Fail($"Expected 1 human after drop; got {worldAfter.HumanPlayerCount}.");

        return PlaybookResult.Success("dropped-to-one");
    }

    private static async Task TapActivate(IPlaybookContext context, CancellationToken cancellationToken)
    {
        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
    }

    private static async Task TapJoy(
        IPlaybookContext context,
        int device,
        JoyButton button,
        CancellationToken cancellationToken)
    {
        await context.SetJoypadButtonAsync(device, (int)button, true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetJoypadButtonAsync(device, (int)button, false, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
    }
}
