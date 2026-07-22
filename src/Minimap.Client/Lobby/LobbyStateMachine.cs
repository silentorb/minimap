using Minimap.Client.LocalPlay;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>Pure lobby slot state: Available → Claimed → Ready.</summary>
public sealed class LobbyStateMachine
{
    public const int SlotCount = 4;

    private readonly LobbySlotMode[] _modes = new LobbySlotMode[SlotCount];
    private readonly LobbySlotBinding[] _bindings = new LobbySlotBinding[SlotCount];
    private readonly LobbyAccessorySelectionState?[] _accessorySelections =
        new LobbyAccessorySelectionState?[SlotCount];
    private int _accessoryPoints = 2;
    private IReadOnlyList<AccessoryDefinition> _selectableAccessories =
        Array.Empty<AccessoryDefinition>();

    public LobbyStateMachine()
    {
        for (var i = 0; i < SlotCount; i++)
            _bindings[i] = new LobbySlotBinding();
    }

    public void ConfigureAccessories(
        int accessoryPoints,
        IReadOnlyList<AccessoryDefinition> selectableAccessories)
    {
        if (accessoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(accessoryPoints));
        ArgumentNullException.ThrowIfNull(selectableAccessories);
        _accessoryPoints = accessoryPoints;
        _selectableAccessories = selectableAccessories;
    }

    public IReadOnlyList<AccessoryDefinition> SelectableAccessories => _selectableAccessories;

    public int AccessoryPoints => _accessoryPoints;

    public LobbyAccessorySelectionState? GetAccessorySelection(int slotIndex) =>
        _accessorySelections[slotIndex];

    public LobbySlotMode GetMode(int slotIndex) => _modes[slotIndex];

    public IReadOnlyCollection<InputDeviceId> GetDevices(int slotIndex) =>
        _bindings[slotIndex].Devices;

    public int ClaimedCount
    {
        get
        {
            var n = 0;
            for (var i = 0; i < SlotCount; i++)
            {
                if (_modes[i] is LobbySlotMode.Claimed or LobbySlotMode.Ready)
                    n++;
            }

            return n;
        }
    }

    public bool CanStartGame
    {
        get
        {
            var claimed = 0;
            for (var i = 0; i < SlotCount; i++)
            {
                if (_modes[i] == LobbySlotMode.Available)
                    continue;
                if (_modes[i] != LobbySlotMode.Ready)
                    return false;
                claimed++;
            }

            return claimed > 0;
        }
    }

    public bool IsDeviceBound(InputDeviceId device)
    {
        for (var i = 0; i < SlotCount; i++)
        {
            if (_bindings[i].Contains(device))
                return true;
        }

        return false;
    }

    public int? FindSlotForDevice(InputDeviceId device)
    {
        for (var i = 0; i < SlotCount; i++)
        {
            if (_bindings[i].Contains(device))
                return i;
        }

        return null;
    }

    public bool TryClaim(InputDeviceId device, out int slotIndex)
    {
        slotIndex = -1;
        if (IsDeviceBound(device))
            return false;

        for (var i = 0; i < SlotCount; i++)
        {
            if (_modes[i] != LobbySlotMode.Available)
                continue;
            _modes[i] = LobbySlotMode.Claimed;
            _bindings[i].Add(device);
            _accessorySelections[i] = new LobbyAccessorySelectionState(_accessoryPoints);
            slotIndex = i;
            return true;
        }

        return false;
    }

    public bool TryReady(InputDeviceId device)
    {
        var slot = FindSlotForDevice(device);
        if (slot is not int s)
            return false;
        if (_modes[s] != LobbySlotMode.Claimed)
            return false;
        _modes[s] = LobbySlotMode.Ready;
        return true;
    }

    public bool TryBack(InputDeviceId device)
    {
        var slot = FindSlotForDevice(device);
        if (slot is not int s)
            return false;

        switch (_modes[s])
        {
            case LobbySlotMode.Ready:
                _modes[s] = LobbySlotMode.Claimed;
                return true;
            case LobbySlotMode.Claimed:
                _modes[s] = LobbySlotMode.Available;
                _bindings[s].Clear();
                _accessorySelections[s] = null;
                return true;
            default:
                return false;
        }
    }

    public LocalPlayRoster BuildRoster()
    {
        var roster = new LocalPlayRoster();
        var count = 0;
        for (var i = 0; i < SlotCount; i++)
        {
            if (_modes[i] is LobbySlotMode.Claimed or LobbySlotMode.Ready)
                count = i + 1;
        }

        roster.SetPlayerCount(count);
        for (var i = 0; i < count; i++)
        {
            foreach (var d in _bindings[i].Devices)
                roster.Players[i].AddDevice(d);

            var selection = _accessorySelections[i];
            if (selection is not null)
                roster.Players[i].SetSelectedAccessories(selection.Owned);
        }

        return roster;
    }

    public void Reset()
    {
        for (var i = 0; i < SlotCount; i++)
        {
            _modes[i] = LobbySlotMode.Available;
            _bindings[i].Clear();
            _accessorySelections[i] = null;
        }
    }
}
