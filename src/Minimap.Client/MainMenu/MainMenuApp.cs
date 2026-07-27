using Godot;

namespace Minimap.Client.MainMenu;

/// <summary>Start / main menu screen: title, New → lobby, Profiles, Quit.</summary>
public partial class MainMenuApp : Control
{
    public const string LobbyScenePath = "res://scenes/lobby.tscn";
    public const string ProfilesScenePath = "res://scenes/profiles.tscn";
    public const string TitleText = "CompuQuest Mini";

    private readonly MainMenuScreenController _controller = new();
    private Label? _title;
    private Button? _newButton;
    private Button? _profilesButton;
    private Button? _quitButton;

    public Label? TitleLabel => _title;
    public Button? NewButton => _newButton;
    public Button? ProfilesButton => _profilesButton;
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
        _profilesButton = GetNode<Button>("Center/VBox/ProfilesButton");
        _quitButton = GetNode<Button>("Center/VBox/QuitButton");
        _title.Text = TitleText;
        _newButton.Pressed += () => InvokeAction(MainMenuAction.New);
        _profilesButton.Pressed += () => InvokeAction(MainMenuAction.Profiles);
        _quitButton.Pressed += () => InvokeAction(MainMenuAction.Quit);
        SyncFocus();
    }

    public override void _Input(InputEvent @event)
    {
        // Before GUI so D-pad / activate stay aligned with _controller (same pattern as popup).
        if (@event is InputEventKey key && !key.Echo && key.Pressed)
        {
            if (TryHandleInput(key.Keycode, null))
                GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventJoypadButton joy && joy.Pressed)
        {
            if (TryHandleInput(null, joy.ButtonIndex))
                GetViewport().SetInputAsHandled();
        }
    }

    private bool TryHandleInput(Key? key, JoyButton? button)
    {
        Decode(key, button, out var delta, out var activate);
        if (delta == 0 && !activate)
            return false;

        if (!_controller.TryHandle(delta, activate, out var activated))
            return false;

        if (activated is MainMenuAction action)
        {
            InvokeAction(action);
            return true;
        }

        SyncFocus();
        return true;
    }

    private void InvokeAction(MainMenuAction action)
    {
        switch (action)
        {
            case MainMenuAction.New:
                ChangeSceneOrThrow(LobbyScenePath);
                break;
            case MainMenuAction.Profiles:
                ChangeSceneOrThrow(ProfilesScenePath);
                break;
            case MainMenuAction.Quit:
                GetTree().Quit();
                break;
        }
    }

    private void SyncFocus()
    {
        var button = ButtonFor(_controller.SelectedAction);
        button?.GrabFocus();
    }

    private Button? ButtonFor(MainMenuAction action) =>
        action switch
        {
            MainMenuAction.New => _newButton,
            MainMenuAction.Profiles => _profilesButton,
            MainMenuAction.Quit => _quitButton,
            _ => null,
        };

    private static void Decode(Key? key, JoyButton? button, out int navigateDelta, out bool activateSelected)
    {
        navigateDelta = 0;
        activateSelected = false;

        if (key is Key k)
        {
            navigateDelta = k switch
            {
                Key.Up => -1,
                Key.Down => 1,
                _ => 0,
            };
            if (navigateDelta != 0)
                return;

            if (GameInput.IsActivateKey(k))
                activateSelected = true;
            return;
        }

        if (button is JoyButton b)
        {
            navigateDelta = b switch
            {
                JoyButton.DpadUp => -1,
                JoyButton.DpadDown => 1,
                _ => 0,
            };
            if (navigateDelta != 0)
                return;

            if (GameInput.IsActivateButton(b))
                activateSelected = true;
        }
    }

    private void ChangeSceneOrThrow(string path)
    {
        var error = GetTree().ChangeSceneToFile(path);
        if (error == Error.Ok)
            return;

        GD.PushError($"ChangeSceneToFile failed for '{path}': {error}");
        throw new InvalidOperationException($"ChangeSceneToFile failed for '{path}' with {error}.");
    }
}
