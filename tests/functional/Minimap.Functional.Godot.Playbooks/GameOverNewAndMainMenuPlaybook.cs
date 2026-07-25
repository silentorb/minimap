using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Game over shows New game / Main menu; each navigates correctly.</summary>
public sealed class GameOverNewAndMainMenuPlaybook : IPlaybook
{
    public string Id => "GameOverNewAndMainMenu";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/world.tscn", cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        await context.ForceGameOverAsync(cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        var newGame = await context.GetControlRectAsync(
            "GameOverOverlay/Center/Panel/Margin/VBox/NewGameButton",
            cancellationToken);
        var mainMenu = await context.GetControlRectAsync(
            "GameOverOverlay/Center/Panel/Margin/VBox/MainMenuButton",
            cancellationToken);
        if (!newGame.Found || !newGame.Visible)
            return PlaybookResult.Fail("Expected New game button on game over overlay.");
        if (!mainMenu.Found || !mainMenu.Visible)
            return PlaybookResult.Fail("Expected Main menu button on game over overlay.");

        await context.PressButtonAsync(
            "GameOverOverlay/Center/Panel/Margin/VBox/MainMenuButton",
            cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var title = await context.GetLabelTextAsync("Center/VBox/Title", cancellationToken);
        if (title != "CompuQuest Mini")
            return PlaybookResult.Fail($"Expected main menu after Main menu action; title='{title}'.");

        await context.LoadSceneAsync("res://scenes/world.tscn", cancellationToken);
        await context.WaitFramesAsync(15, cancellationToken);
        await context.ForceGameOverAsync(cancellationToken);
        await context.WaitFramesAsync(5, cancellationToken);

        await context.PressButtonAsync(
            "GameOverOverlay/Center/Panel/Margin/VBox/NewGameButton",
            cancellationToken);
        await context.WaitFramesAsync(20, cancellationToken);

        var lobby = await context.GetLobbySnapshotAsync(cancellationToken);
        if (!lobby.IsLobbyScene)
            return PlaybookResult.Fail("Expected lobby after New game.");

        return PlaybookResult.Success("game-over-nav");
    }
}