using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Main menu screen shows title and New/Profiles/Quit.</summary>
public sealed class MainMenuBootstrapPlaybook : IPlaybook
{
    public string Id => "MainMenuBootstrap";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/main_menu.tscn", cancellationToken);
        await context.WaitFramesAsync(15, cancellationToken);

        var titleText = await context.GetLabelTextAsync("Center/VBox/Title", cancellationToken);
        if (titleText != "CompuQuest Mini")
            return PlaybookResult.Fail($"Expected title 'CompuQuest Mini'; got '{titleText}'.");

        var newButton = await context.GetControlRectAsync("Center/VBox/NewButton", cancellationToken);
        var profilesButton = await context.GetControlRectAsync("Center/VBox/ProfilesButton", cancellationToken);
        var quitButton = await context.GetControlRectAsync("Center/VBox/QuitButton", cancellationToken);
        if (!newButton.Found || !profilesButton.Found || !quitButton.Found)
            return PlaybookResult.Fail("Expected New, Profiles, and Quit buttons.");

        return PlaybookResult.Success("main-menu-bootstrap");
    }
}
