using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>New on the main menu navigates to the lobby.</summary>
public sealed class MainMenuNewGoesToLobbyPlaybook : IPlaybook
{
    public string Id => "MainMenuNewGoesToLobby";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/main_menu.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        await context.SetActivateKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetActivateKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var snap = await context.GetLobbySnapshotAsync(cancellationToken);
        if (!snap.IsLobbyScene)
            return PlaybookResult.Fail("Expected lobby after New.");
        if (snap.SlotModes.Count != 4 || snap.SlotModes.Any(m => m != "Available"))
            return PlaybookResult.Fail($"Expected four Available panels; got {string.Join(',', snap.SlotModes)}.");

        return PlaybookResult.Success("new-to-lobby");
    }
}
