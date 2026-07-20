using Godot;

namespace Minimap.Automation;

/// <summary>
/// Optional surface for nodes that expose held movement-key state to automation helpers.
/// Implement on gameplay views that already track keyboard motion.
/// </summary>
public interface IMovementKeyTarget
{
    void SetMovementKeyState(Key key, bool pressed);
}
