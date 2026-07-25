using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Escape opens the main menu popup (paused); Escape again continues.</summary>
public sealed class MainMenuPopupPauseAndContinuePlaybook : IPlaybook
{
    public string Id => "MainMenuPopupPauseAndContinue";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/world.tscn", cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var world = await context.GetWorldSnapshotAsync(cancellationToken);
        if (!world.IsWorldRoot)
            return PlaybookResult.Fail("Expected world scene.");

        await context.SetBackKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetBackKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        var paused = await context.GetPauseOverlaySnapshotAsync(cancellationToken);
        if (!paused.MainMenuVisible || !paused.TreePaused)
            return PlaybookResult.Fail(
                $"Expected main menu popup paused; mainMenu={paused.MainMenuVisible} treePaused={paused.TreePaused}.");

        await context.SetBackKeyAsync(true, cancellationToken);
        await context.WaitFramesAsync(2, cancellationToken);
        await context.SetBackKeyAsync(false, cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        var resumed = await context.GetPauseOverlaySnapshotAsync(cancellationToken);
        if (resumed.MainMenuVisible || resumed.TreePaused)
            return PlaybookResult.Fail(
                $"Expected resume after Continue; mainMenu={resumed.MainMenuVisible} treePaused={resumed.TreePaused}.");

        return PlaybookResult.Success("popup-pause-continue");
    }
}
