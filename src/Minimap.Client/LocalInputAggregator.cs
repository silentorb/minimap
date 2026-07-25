using Godot;
using Minimap.Client.LocalPlay;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Merged per-player in-world input for one frame.</summary>
public readonly struct PlayerWorldInput
{
    public PlayerWorldInput(
        SimVec2 move,
        SimVec2 aim,
        bool fireHeld,
        bool secondaryFireHeld,
        bool abilityActivatePressed,
        bool abilityBackPressed,
        bool interactPressed,
        int? modalSelect)
    {
        Move = move;
        Aim = aim;
        FireHeld = fireHeld;
        SecondaryFireHeld = secondaryFireHeld;
        AbilityActivatePressed = abilityActivatePressed;
        AbilityBackPressed = abilityBackPressed;
        InteractPressed = interactPressed;
        ModalSelect = modalSelect;
    }

    public SimVec2 Move { get; }
    public SimVec2 Aim { get; }
    public bool FireHeld { get; }
    public bool SecondaryFireHeld { get; }
    public bool AbilityActivatePressed { get; }
    public bool AbilityBackPressed { get; }
    public bool InteractPressed { get; }
    public int? ModalSelect { get; }
}

/// <summary>Merges move/aim/fire/ability input from all devices bound to each local player.</summary>
public sealed class LocalInputAggregator
{
    private const float AxisDeadzone = 0.25f;
    private const float TriggerDeadzone = 0.35f;

    private readonly LocalPlayRoster _roster;
    private readonly WorldView? _worldView;
    private readonly Dictionary<int, ButtonEdgeState> _edges = new();

    public LocalInputAggregator(LocalPlayRoster roster, WorldView? worldView)
    {
        _roster = roster;
        _worldView = worldView;
    }

    public PlayerWorldInput ReadPlayer(int playerIndex, SimVec2 aimOrigin)
    {
        if (playerIndex < 0 || playerIndex >= _roster.PlayerCount)
            return default;

        var move = ReadMerged(playerIndex, ReadKeyboardMove, ReadJoypadMove);
        var aim = ReadMerged(playerIndex, () => ReadKeyboardAim(aimOrigin), ReadJoypadAim);
        var fireHeld = false;
        var secondaryFireHeld = false;
        var activateHeld = false;
        var backHeld = false;
        var interactHeld = false;
        int? modalSelect = null;

        foreach (var device in _roster.Players[playerIndex].Devices)
        {
            if (device.IsKeyboard)
            {
                fireHeld |= Input.IsMouseButtonPressed(MouseButton.Left);
                secondaryFireHeld |= Input.IsMouseButtonPressed(MouseButton.Right);
                activateHeld |= Input.IsKeyPressed(Key.Space);
                backHeld |= Input.IsKeyPressed(Key.Escape);
                interactHeld |= Input.IsKeyPressed(Key.E);
                modalSelect ??= ReadKeyboardModalSelect();
            }
            else if (Input.GetConnectedJoypads().Contains(device.JoypadDevice))
            {
                var rightTrigger = Input.GetJoyAxis(device.JoypadDevice, JoyAxis.TriggerRight);
                fireHeld |= rightTrigger >= TriggerDeadzone;
                var leftTrigger = Input.GetJoyAxis(device.JoypadDevice, JoyAxis.TriggerLeft);
                secondaryFireHeld |= leftTrigger >= TriggerDeadzone;
                activateHeld |= Input.IsJoyButtonPressed(device.JoypadDevice, JoyButton.X);
                backHeld |= Input.IsJoyButtonPressed(device.JoypadDevice, JoyButton.B);
                interactHeld |= Input.IsJoyButtonPressed(device.JoypadDevice, JoyButton.A);
                modalSelect ??= ReadJoypadModalSelect(device.JoypadDevice);
            }
        }

        if (!_edges.TryGetValue(playerIndex, out var edge))
        {
            edge = new ButtonEdgeState();
            _edges[playerIndex] = edge;
        }

        var activatePressed = activateHeld && !edge.ActivateWasHeld;
        var backPressed = backHeld && !edge.BackWasHeld;
        var interactPressed = interactHeld && !edge.InteractWasHeld;
        var modalPressed = modalSelect is int slot && edge.LastModalSelect != slot
            ? modalSelect
            : null;
        if (modalSelect is null)
            edge.LastModalSelect = null;
        else
            edge.LastModalSelect = modalSelect;

        edge.ActivateWasHeld = activateHeld;
        edge.BackWasHeld = backHeld;
        edge.InteractWasHeld = interactHeld;

        return new PlayerWorldInput(
            move,
            aim,
            fireHeld,
            secondaryFireHeld,
            activatePressed,
            backPressed,
            interactPressed,
            modalPressed);
    }

    private SimVec2 ReadMerged(
        int playerIndex,
        Func<SimVec2> keyboard,
        Func<int, SimVec2> joypad)
    {
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

    private SimVec2 ReadKeyboardAim(SimVec2 origin)
    {
        if (_worldView is null)
            return SimVec2.Zero;
        return _worldView.ReadMouseAimFrom(origin);
    }

    private static int? ReadKeyboardModalSelect()
    {
        if (Input.IsKeyPressed(Key.Key1))
            return 0;
        if (Input.IsKeyPressed(Key.Key2))
            return 1;
        if (Input.IsKeyPressed(Key.Key3))
            return 2;
        if (Input.IsKeyPressed(Key.Key4))
            return 3;
        return null;
    }

    private static int? ReadJoypadModalSelect(int deviceIndex)
    {
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadUp))
            return 0;
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadRight))
            return 1;
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadDown))
            return 2;
        if (Input.IsJoyButtonPressed(deviceIndex, JoyButton.DpadLeft))
            return 3;
        return null;
    }

    private static SimVec2 ReadJoypadMove(int deviceIndex)
    {
        if (!Input.GetConnectedJoypads().Contains(deviceIndex))
            return SimVec2.Zero;

        var x = Input.GetJoyAxis(deviceIndex, JoyAxis.LeftX);
        var y = Input.GetJoyAxis(deviceIndex, JoyAxis.LeftY);
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

    private sealed class ButtonEdgeState
    {
        public bool ActivateWasHeld;
        public bool BackWasHeld;
        public bool InteractWasHeld;
        public int? LastModalSelect;
    }
}
