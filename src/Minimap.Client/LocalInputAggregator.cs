using Godot;
using Minimap.Client.LocalPlay;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Merges move and aim input from all devices bound to each local player.</summary>
public sealed class LocalInputAggregator
{
    private const float AxisDeadzone = 0.25f;
    private readonly LocalPlayRoster _roster;
    private readonly WorldView? _worldView;

    public LocalInputAggregator(LocalPlayRoster roster, WorldView? worldView)
    {
        _roster = roster;
        _worldView = worldView;
    }

    public SimVec2 ReadMove(int playerIndex) =>
        ReadMerged(playerIndex, ReadKeyboardMove, ReadJoypadMove);

    public SimVec2 ReadAim(int playerIndex) =>
        ReadMerged(playerIndex, ReadKeyboardAim, ReadJoypadAim);

    private SimVec2 ReadMerged(
        int playerIndex,
        Func<SimVec2> keyboard,
        Func<int, SimVec2> joypad)
    {
        if (playerIndex < 0 || playerIndex >= _roster.PlayerCount)
            return SimVec2.Zero;

        var x = 0f;
        var y = 0f;
        foreach (var device in _roster.Players[playerIndex].Devices)
        {
            if (device.IsKeyboard)
            {
                var kb = keyboard();
                x += kb.X;
                y += kb.Y;
            }
            else
            {
                var pad = joypad(device.JoypadDevice);
                x += pad.X;
                y += pad.Y;
            }
        }

        return ClampAxis(new SimVec2(x, y));
    }

    private SimVec2 ReadKeyboardMove()
    {
        if (_worldView is null)
            return SimVec2.Zero;
        return _worldView.ReadMoveInput();
    }

    private SimVec2 ReadKeyboardAim()
    {
        if (_worldView is null)
            return SimVec2.Zero;
        return _worldView.ReadAimInput();
    }

    private static SimVec2 ReadJoypadMove(int deviceIndex)
    {
        if (!Input.GetConnectedJoypads().Contains(deviceIndex))
            return SimVec2.Zero;

        var x = Input.GetJoyAxis(deviceIndex, JoyAxis.LeftX);
        var y = Input.GetJoyAxis(deviceIndex, JoyAxis.LeftY);

        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadRight))
            x += 1f;
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadLeft))
            x -= 1f;
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadDown))
            y += 1f;
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadUp))
            y -= 1f;

        return ClampAxis(new SimVec2(ApplyDeadzone(x), ApplyDeadzone(y)));
    }

    private static SimVec2 ReadJoypadAim(int deviceIndex)
    {
        if (!Input.GetConnectedJoypads().Contains(deviceIndex))
            return SimVec2.Zero;

        var x = Input.GetJoyAxis(deviceIndex, JoyAxis.RightX);
        var y = Input.GetJoyAxis(deviceIndex, JoyAxis.RightY);
        return ClampAxis(new SimVec2(ApplyDeadzone(x), ApplyDeadzone(y)));
    }

    private static float ApplyDeadzone(float v) =>
        Math.Abs(v) < AxisDeadzone ? 0f : v;

    private static SimVec2 ClampAxis(SimVec2 v)
    {
        if (v.LengthSquared < 1e-10f)
            return SimVec2.Zero;
        if (v.LengthSquared <= 1f)
            return v;
        return v.Normalized();
    }
}
