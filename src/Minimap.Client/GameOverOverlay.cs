using Godot;

namespace Minimap.Client;

/// <summary>Pause overlay when all human players have died.</summary>
public partial class GameOverOverlay : CanvasLayer
{
    private Button? _continueButton;

    public event Action? ContinueRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        _continueButton = GetNode<Button>("Center/Panel/Margin/VBox/ContinueButton");
        _continueButton.Pressed += () => ContinueRequested?.Invoke();
    }

    public void ShowOverlay() => Visible = true;

    public void HideOverlay() => Visible = false;

    public bool OverlayVisible => Visible;
}
