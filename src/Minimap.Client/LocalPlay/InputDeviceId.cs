namespace Minimap.Client.LocalPlay;

/// <summary>Identifies a local input device (keyboard or joypad index).</summary>
public readonly record struct InputDeviceId
{
    public bool IsKeyboard { get; init; }
    public int JoypadDevice { get; init; }

    public static InputDeviceId Keyboard { get; } = new() { IsKeyboard = true, JoypadDevice = -1 };

    public static InputDeviceId Joypad(int deviceIndex) =>
        new() { IsKeyboard = false, JoypadDevice = deviceIndex };

    public bool IsJoypad => !IsKeyboard;

    public override string ToString() =>
        IsKeyboard ? "Keyboard" : $"Joypad({JoypadDevice})";
}
