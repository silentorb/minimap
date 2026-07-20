using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client;

/// <summary>Pause overlay when a player's joypad disconnects.</summary>
public partial class ReconnectOverlay : CanvasLayer
{
    private Label? _message;
    private Button? _dropButton;
    private bool _dropFocused;

    public event Action? DropPlayerRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        _message = GetNode<Label>("Center/Panel/Margin/VBox/Message");
        _dropButton = GetNode<Button>("Center/Panel/Margin/VBox/DropButton");
        _dropButton.Pressed += () => DropPlayerRequested?.Invoke();
    }

    public void ShowForPlayer(int playerIndex, bool showDrop)
    {
        if (_message is null || _dropButton is null)
            return;

        _message.Text = showDrop
            ? $"Player {playerIndex + 1}: reconnect controller (A/Start) or another player may drop them."
            : $"Player {playerIndex + 1}: reconnect controller (press A/Start on a free pad).";
        _dropButton.Visible = showDrop;
        _dropFocused = false;
        Visible = true;
    }

    public void HideOverlay() => Visible = false;

    public bool OverlayVisible => Visible;

    public bool DropButtonVisible => _dropButton?.Visible ?? false;

    public void FocusDropForAutomation() => _dropFocused = true;

    public bool TryHandleActivate(InputDeviceId device, Key? key, JoyButton? button)
    {
        if (!Visible || !_dropFocused)
            return false;

        if (key is Key k && GameInput.IsActivateKey(k))
        {
            DropPlayerRequested?.Invoke();
            return true;
        }

        if (button is JoyButton b && GameInput.IsActivateButton(b))
        {
            DropPlayerRequested?.Invoke();
            return true;
        }

        return false;
    }
}
