using Godot;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>
/// Solo joypad opens the main menu popup and activates End game via D-pad + A
/// (regression: popup UI was keyboard/mouse-only).
/// </summary>
public sealed class MainMenuPopupJoypadNavigatePlaybook : IPlaybook
{
    public string Id => "MainMenuPopupJoypadNavigate";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        PlaybookProfileSeed.EnsureProfiles("Playbook");
        await context.ClearLocalPlayContextAsync(cancellationToken);

        await context.LoadSceneAsync("res://scenes/lobby.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await TapJoy(context, 0, JoyButton.A, cancellationToken);
        var claimed = await context.GetLobbySnapshotAsync(cancellationToken);
        if (claimed.SlotModes.FirstOrDefault() != "SelectingProfile")
            return PlaybookResult.Fail($"Expected SelectingProfile; got {claimed.SlotModes.FirstOrDefault()}.");

        await TapJoy(context, 0, JoyButton.Start, cancellationToken);
        var accessories = await context.GetLobbySnapshotAsync(cancellationToken);
        if (accessories.SlotModes.FirstOrDefault() != "SelectingAccessories")
        {
            return PlaybookResult.Fail(
                $"Expected SelectingAccessories; got {accessories.SlotModes.FirstOrDefault()}.");
        }

        await TapJoy(context, 0, JoyButton.Start, cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var world = await context.GetWorldSnapshotAsync(cancellationToken);
        if (!world.IsWorldRoot)
            return PlaybookResult.Fail("Expected world after joypad ready.");

        await TapJoy(context, 0, JoyButton.Start, cancellationToken);
        var paused = await context.GetPauseOverlaySnapshotAsync(cancellationToken);
        if (!paused.MainMenuVisible || !paused.TreePaused)
        {
            return PlaybookResult.Fail(
                $"Expected main menu popup; mainMenu={paused.MainMenuVisible} treePaused={paused.TreePaused}.");
        }

        // Continue → End game, then activate.
        await TapJoy(context, 0, JoyButton.DpadDown, cancellationToken);
        await TapJoy(context, 0, JoyButton.A, cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        var title = await context.GetLabelTextAsync(
            "PostSessionOverlay/Root/Margin/VBox/Title",
            cancellationToken);
        if (!string.Equals(title, "Session complete", StringComparison.Ordinal))
        {
            return PlaybookResult.Fail(
                $"Expected post-session title 'Session complete' after joypad End game, got '{title ?? "<null>"}'.");
        }

        var after = await context.GetPauseOverlaySnapshotAsync(cancellationToken);
        if (after.MainMenuVisible)
            return PlaybookResult.Fail("Main menu popup should hide after End game.");

        await context.ClearLocalPlayContextAsync(cancellationToken);
        return PlaybookResult.Success("joypad-popup-end-game");
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
