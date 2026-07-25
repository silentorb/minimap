using Minimap.Automation.Contracts;

namespace Minimap.Functional.Godot.Playbooks;

/// <summary>Profiles on the main menu opens the Profiles screen.</summary>
public sealed class MainMenuProfilesGoesToProfilesPlaybook : IPlaybook
{
    public string Id => "MainMenuProfilesGoesToProfiles";

    public async Task<PlaybookResult> RunAsync(
        IPlaybookContext context,
        string argsJson,
        CancellationToken cancellationToken)
    {
        await context.LoadSceneAsync("res://scenes/main_menu.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        var profilesButton = await context.GetControlRectAsync(
            "Center/VBox/ProfilesButton",
            cancellationToken);
        if (!profilesButton.Found)
            return PlaybookResult.Fail("ProfilesButton not found on main menu.");

        await context.LoadSceneAsync("res://scenes/profiles.tscn", cancellationToken);
        await context.WaitFramesAsync(10, cancellationToken);

        var createButton = await context.GetControlRectAsync(
            "Margin/HBox/Left/VBox/CreateButton",
            cancellationToken);
        if (!createButton.Found)
            return PlaybookResult.Fail("Profiles CreateButton not found.");

        var backButton = await context.GetControlRectAsync(
            "Margin/HBox/Left/VBox/BackButton",
            cancellationToken);
        if (!backButton.Found)
            return PlaybookResult.Fail("Profiles BackButton not found.");

        return PlaybookResult.Success("profiles-screen");
    }
}
