using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client.MainMenu;

/// <summary>In-world main menu overlay with exclusive owner control.</summary>
public partial class MainMenuPopup : CanvasLayer
{
    public const float OverlayAlpha = 0.6f;

    private ColorRect? _dimmer;
    private Button? _continueButton;
    private Button? _endGameButton;
    private Button? _quitButton;
    private readonly MainMenuPopupController _controller = new();

    public event Action? ContinueRequested;
    public event Action? EndGameRequested;
    public event Action? QuitRequested;

    public MainMenuOwnership Ownership => _controller.Ownership;
    public bool OverlayVisible => Visible;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        _dimmer = GetNode<ColorRect>("Dimmer");
        _dimmer.Color = new Color(0f, 0f, 0f, OverlayAlpha);
        _continueButton = GetNode<Button>("Center/Panel/Margin/VBox/ContinueButton");
        _endGameButton = GetNodeOrNull<Button>("Center/Panel/Margin/VBox/EndGameButton")
            ?? GetNode<Button>("Center/Panel/Margin/VBox/NewButton");
        _quitButton = GetNode<Button>("Center/Panel/Margin/VBox/QuitButton");
        _continueButton.Pressed += () => TryInvokeFromPointer(MainMenuAction.Continue);
        _endGameButton.Pressed += () => TryInvokeFromPointer(MainMenuAction.EndGame);
        _quitButton.Pressed += () => TryInvokeFromPointer(MainMenuAction.Quit);
        if (_endGameButton.Text != "End game")
            _endGameButton.Text = "End game";
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible || !_controller.IsOpen)
            return;

        if (@event is InputEventMouseButton mouse && mouse.Pressed)
        {
            // Mouse belongs to the keyboard device; block it when a joypad owns the menu.
            if (!_controller.Ownership.Accepts(InputDeviceId.Keyboard))
                GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventKey key && !key.Echo && key.Pressed)
        {
            if (TryHandleOwnerInput(GameInput.DeviceFromKeyEvent(key), key.Keycode, null))
                GetViewport().SetInputAsHandled();
            else if (!_controller.Ownership.Accepts(InputDeviceId.Keyboard))
                GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventJoypadButton joy && joy.Pressed)
        {
            var device = GameInput.DeviceFromJoyEvent(joy);
            if (TryHandleOwnerInput(device, null, joy.ButtonIndex))
                GetViewport().SetInputAsHandled();
            else if (!_controller.Ownership.Accepts(device))
                GetViewport().SetInputAsHandled();
        }
    }

    public void ShowForOwner(InputDeviceId owner)
    {
        _controller.Open(owner);
        if (_continueButton is not null)
            _continueButton.Visible = true;
        Visible = true;
        SyncFocus();
    }

    public void HideOverlay()
    {
        Visible = false;
        _controller.Close();
    }

    /// <summary>Owner Start/Escape dismiss (Continue). Kept for WorldApp modal routing.</summary>
    public bool TryHandleOwnerDismiss(InputDeviceId device, Key? key, JoyButton? button) =>
        TryHandleOwnerInput(device, key, button);

    public bool TryHandleOwnerInput(InputDeviceId device, Key? key, JoyButton? button)
    {
        Decode(key, button, out var delta, out var activate, out var dismiss);
        if (!dismiss && delta == 0 && !activate)
            return false;

        if (!_controller.TryHandle(device, delta, activate, dismiss, out var activated))
            return false;

        if (activated is MainMenuAction action)
        {
            InvokeAction(action);
            return true;
        }

        SyncFocus();
        return true;
    }

    private void TryInvokeFromPointer(MainMenuAction action)
    {
        // Button.Pressed is mouse/keyboard GUI; only when keyboard owns (or no exclusive joypad).
        if (!_controller.Ownership.Accepts(InputDeviceId.Keyboard))
            return;
        InvokeAction(action);
    }

    private void InvokeAction(MainMenuAction action)
    {
        switch (action)
        {
            case MainMenuAction.Continue:
                ContinueRequested?.Invoke();
                break;
            case MainMenuAction.EndGame:
                EndGameRequested?.Invoke();
                break;
            case MainMenuAction.Quit:
                QuitRequested?.Invoke();
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
            MainMenuAction.Continue => _continueButton,
            MainMenuAction.EndGame => _endGameButton,
            MainMenuAction.Quit => _quitButton,
            _ => null,
        };

    private static void Decode(
        Key? key,
        JoyButton? button,
        out int navigateDelta,
        out bool activateSelected,
        out bool dismiss)
    {
        navigateDelta = 0;
        activateSelected = false;
        dismiss = false;

        if (key is Key k)
        {
            if (GameInput.IsMenuOpenKey(k))
            {
                dismiss = true;
                return;
            }

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
            if (GameInput.IsMenuOpenButton(b))
            {
                dismiss = true;
                return;
            }

            navigateDelta = b switch
            {
                JoyButton.DpadUp => -1,
                JoyButton.DpadDown => 1,
                _ => 0,
            };
            if (navigateDelta != 0)
                return;

            // A activates the focused option; Start is dismiss (handled above).
            if (b is JoyButton.A)
                activateSelected = true;
        }
    }
}
