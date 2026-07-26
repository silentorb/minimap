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
    private readonly MainMenuOwnership _ownership = new();

    public event Action? ContinueRequested;
    public event Action? EndGameRequested;
    public event Action? QuitRequested;

    public MainMenuOwnership Ownership => _ownership;
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
        _continueButton.Pressed += () => ContinueRequested?.Invoke();
        _endGameButton.Pressed += () => EndGameRequested?.Invoke();
        _quitButton.Pressed += () => QuitRequested?.Invoke();
        if (_endGameButton.Text != "End game")
            _endGameButton.Text = "End game";
    }

    public void ShowForOwner(InputDeviceId owner)
    {
        _ownership.Open(owner);
        if (_continueButton is not null)
            _continueButton.Visible = true;
        Visible = true;
    }

    public void HideOverlay()
    {
        Visible = false;
        _ownership.Close();
    }

    public bool TryHandleOwnerDismiss(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (!_ownership.IsOpen || !_ownership.Accepts(device))
            return false;

        var dismiss = key is Key k && GameInput.IsMenuOpenKey(k)
            || button is JoyButton b && GameInput.IsMenuOpenButton(b);
        if (!dismiss)
            return false;

        ContinueRequested?.Invoke();
        return true;
    }
}
