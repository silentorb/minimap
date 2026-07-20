using Godot;
using Minimap.Client.LocalPlay;

namespace Minimap.Client;

/// <summary>Universal joypad + keyboard lobby/world action detection.</summary>
public static class GameInput
{
    public static bool IsActivateKey(Key key) =>
        key is Key.Enter or Key.KpEnter or Key.Space;

    public static bool IsBackKey(Key key) => key is Key.Escape;

    public static bool IsActivateButton(JoyButton button) =>
        button is JoyButton.A or JoyButton.Start;

    public static bool IsReadyButton(JoyButton button) => button is JoyButton.Start;

    public static bool IsBackButton(JoyButton button) => button is JoyButton.B;

    public static InputDeviceId DeviceFromKeyEvent(InputEventKey key) => InputDeviceId.Keyboard;

    public static InputDeviceId DeviceFromJoyEvent(InputEventJoypadButton joy) =>
        InputDeviceId.Joypad(joy.Device);

    public static bool IsJoypadConnected(int deviceIndex) =>
        Input.GetConnectedJoypads().Contains(deviceIndex);
}
