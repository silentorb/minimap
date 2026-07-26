using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Game over opens post-session; all-ready returns to lobby (not main menu).</summary>
public sealed class GameOverNewAndMainMenuPlaybook : IPlaybook
{
    public string Id => "GameOverNewAndMainMenu";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/world.tscn", cancellationToken);
        await context.WaitFramesAsync(8, cancellationToken);

        await context.ForceGameOverAsync(cancellationToken);
        await context.WaitFramesAsync(4, cancellationToken);

        var title = await context.GetLabelTextAsync(
            "PostSessionOverlay/Root/Margin/VBox/Title",
            cancellationToken);
        if (!string.Equals(title, "Game Over", StringComparison.Ordinal))
        {
            return PlaybookResult.Fail(
                $"Expected post-session title 'Game Over', got '{title ?? "<null>"}'.");
        }

        await context.ForcePostSessionAllReadyAsync(cancellationToken);
        await context.WaitFramesAsync(8, cancellationToken);

        var lobby = await context.GetLobbySnapshotAsync(cancellationToken);
        if (!lobby.IsLobbyScene)
            return PlaybookResult.Fail("Expected lobby after post-session all-ready.");

        return PlaybookResult.Success("post-session-to-lobby");
    }
}
