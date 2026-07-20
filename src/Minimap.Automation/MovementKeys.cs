using Godot;

namespace Minimap.Automation;

/// <summary>Helpers for driving movement-key state on <see cref="IMovementKeyTarget"/> nodes.</summary>
public static class MovementKeys
{
    public static void Set(IMovementKeyTarget target, Key key, bool pressed)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetMovementKeyState(key, pressed);
    }

    public static void Press(IMovementKeyTarget target, Key key) => Set(target, key, true);

    public static void Release(IMovementKeyTarget target, Key key) => Set(target, key, false);
}
