namespace Minimap.Client.MainMenu;

/// <summary>Menu actions shared by the main menu screen and in-world popup.</summary>
public enum MainMenuAction
{
    Continue,
    New,
    Quit,
}

/// <summary>Godot-free option lists for the main menu.</summary>
public static class MainMenuModel
{
    public static IReadOnlyList<MainMenuAction> ScreenOptions { get; } =
        new[] { MainMenuAction.New, MainMenuAction.Quit };

    public static IReadOnlyList<MainMenuAction> PopupOptions(bool activeGame) =>
        activeGame
            ? new[] { MainMenuAction.Continue, MainMenuAction.New, MainMenuAction.Quit }
            : new[] { MainMenuAction.New, MainMenuAction.Quit };
}
