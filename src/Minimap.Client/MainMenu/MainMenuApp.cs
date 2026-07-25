using Godot;

namespace Minimap.Client.MainMenu;

/// <summary>Start / main menu screen: title, New → lobby, Quit.</summary>
public partial class MainMenuApp : Control
{
    public const string LobbyScenePath = "res://scenes/lobby.tscn";
    public const string TitleText = "CompuQuest Mini";

    private Label? _title;
    private Button? _newButton;
    private Button? _quitButton;

    public Label? TitleLabel => _title;
    public Button? NewButton => _newButton;
    public Button? QuitButton => _quitButton;

    public override void _Ready()
    {
        if (WorldHostHooks.ResolveShouldStartAtLobby())
        {
            ChangeSceneOrThrow(LobbyScenePath);
            return;
        }

        _title = GetNode<Label>("Center/VBox/Title");
        _newButton = GetNode<Button>("Center/VBox/NewButton");
        _quitButton = GetNode<Button>("Center/VBox/QuitButton");
        _title.Text = TitleText;
        _newButton.Pressed += OnNewPressed;
        _quitButton.Pressed += OnQuitPressed;
        _newButton.GrabFocus();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey key || key.Echo || !key.Pressed)
            return;

        if (GameInput.IsActivateKey(key.Keycode))
        {
            OnNewPressed();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnNewPressed() => ChangeSceneOrThrow(LobbyScenePath);

    private void OnQuitPressed() => GetTree().Quit();

    private void ChangeSceneOrThrow(string path)
    {
        var error = GetTree().ChangeSceneToFile(path);
        if (error == Error.Ok)
            return;

        GD.PushError($"ChangeSceneToFile failed for '{path}': {error}");
        throw new InvalidOperationException($"ChangeSceneToFile failed for '{path}' with {error}.");
    }
}
