using Godot;
using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>
/// Joypad A on the main menu activates New (regression: screen only handled keyboard activate).
/// </summary>
public sealed class MainMenuJoypadActivateGoesToLobbyPlaybook : IPlaybook
{
    public string Id => "MainMenuJoypadActivateGoesToLobby";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/main_menu.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await TapJoy(context, 0, JoyButton.A, cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var snap = await context.GetLobbySnapshotAsync(cancellationToken);
        if (!snap.IsLobbyScene)
            return PlaybookResult.Fail("Expected lobby after joypad A on main menu New.");

        return PlaybookResult.Success("joypad-new-to-lobby");
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
