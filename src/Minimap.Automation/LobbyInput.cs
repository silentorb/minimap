using Godot;

namespace Minimap.Automation;

/// <summary>Lobby keyboard activate/back injection for playbooks.</summary>
public static class LobbyInput
{
    public static void SetActivateKey(Node treeRoot, bool pressed)
    {
        PushKey(treeRoot, Key.Enter, pressed);
    }

    public static void SetBackKey(Node treeRoot, bool pressed)
    {
        PushKey(treeRoot, Key.Escape, pressed);
    }

    public static void SetJoypadButton(Node treeRoot, int deviceIndex, JoyButton button, bool pressed)
    {
        var ev = new InputEventJoypadButton
        {
            Device = deviceIndex,
            ButtonIndex = button,
            Pressed = pressed,
        };
        treeRoot.GetTree()?.Root?.PushInput(ev);
    }

    private static void PushKey(Node treeRoot, Key key, bool pressed)
    {
        var ev = new InputEventKey
        {
            Keycode = key,
            PhysicalKeycode = key,
            Pressed = pressed,
        };
        treeRoot.GetTree()?.Root?.PushInput(ev);
    }
}
