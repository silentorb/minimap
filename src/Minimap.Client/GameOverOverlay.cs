using Godot;

namespace Minimap.Client;

/// <summary>Pause overlay when all human players have died.</summary>
public partial class GameOverOverlay : CanvasLayer
{
    private Button? _newGameButton;
    private Button? _mainMenuButton;

    public event Action? NewGameRequested;
    public event Action? MainMenuRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        _newGameButton = GetNode<Button>("Center/Panel/Margin/VBox/NewGameButton");
        _mainMenuButton = GetNode<Button>("Center/Panel/Margin/VBox/MainMenuButton");
        _newGameButton.Pressed += () => NewGameRequested?.Invoke();
        _mainMenuButton.Pressed += () => MainMenuRequested?.Invoke();
    }

    public void ShowOverlay() => Visible = true;

    public void HideOverlay() => Visible = false;

    public bool OverlayVisible => Visible;
}
