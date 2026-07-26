using Minimap.Client.LocalPlay;
using Minimap.Client.Profiles;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>Pure lobby slot state: Available → SelectingProfile → SelectingAccessories → Ready.</summary>
public sealed class LobbyStateMachine
{
    public const int SlotCount = 4;

    private readonly LobbySlotMode[] _modes = new LobbySlotMode[SlotCount];
    private readonly LobbySlotBinding[] _bindings = new LobbySlotBinding[SlotCount];
    private readonly LobbyProfileSelectionState?[] _profileSelections =
        new LobbyProfileSelectionState?[SlotCount];
    private readonly LobbyAccessorySelectionState?[] _accessorySelections =
        new LobbyAccessorySelectionState?[SlotCount];
    private int _accessoryPoints = 2;
    private IReadOnlyList<AccessoryDefinition> _selectableAccessories =
        Array.Empty<AccessoryDefinition>();
    private IReadOnlyList<DomainDefinition> _domains = Array.Empty<DomainDefinition>();
    private PlayerProfileCatalog _profiles = new();

    public LobbyStateMachine()
    {
        for (var i = 0; i < SlotCount; i++)
            _bindings[i] = new LobbySlotBinding();
    }

    public void ConfigureAccessories(
        int accessoryPoints,
        IReadOnlyList<AccessoryDefinition> selectableAccessories,
        IReadOnlyList<DomainDefinition>? domains = null)
    {
        if (accessoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(accessoryPoints));
        ArgumentNullException.ThrowIfNull(selectableAccessories);
        _accessoryPoints = accessoryPoints;
        _selectableAccessories = selectableAccessories;
        _domains = domains ?? Array.Empty<DomainDefinition>();
    }

    public void ConfigureProfiles(PlayerProfileCatalog profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        _profiles = profiles;
    }

    public PlayerProfileCatalog Profiles => _profiles;

    public IReadOnlyList<AccessoryDefinition> SelectableAccessories => _selectableAccessories;

    public IReadOnlyList<DomainDefinition> Domains => _domains;

    public int AccessoryPoints => _accessoryPoints;

    public LobbyAccessorySelectionState? GetAccessorySelection(int slotIndex) =>
        _accessorySelections[slotIndex];

    public LobbyProfileSelectionState? GetProfileSelection(int slotIndex) =>
        _profileSelections[slotIndex];

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
                if (_modes[i] is LobbySlotMode.SelectingProfile
                    or LobbySlotMode.SelectingAccessories
                    or LobbySlotMode.Ready)
                {
                    n++;
                }
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

    public IReadOnlyList<PlayerProfileRecord> GetAvailableProfilesForSlot(int slotIndex)
    {
        var taken = new HashSet<Guid>();
        for (var i = 0; i < SlotCount; i++)
        {
            if (i == slotIndex)
                continue;
            if (_profileSelections[i]?.ConfirmedProfileId is Guid id)
                taken.Add(id);
        }

        var list = new List<PlayerProfileRecord>();
        foreach (var p in _profiles.Profiles)
        {
            if (!taken.Contains(p.Id))
                list.Add(p);
        }

        return list;
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
            _modes[i] = LobbySlotMode.SelectingProfile;
            _bindings[i].Add(device);
            _profileSelections[i] = new LobbyProfileSelectionState();
            _accessorySelections[i] = new LobbyAccessorySelectionState(_accessoryPoints);
            slotIndex = i;
            return true;
        }

        return false;
    }

    public bool TryCycleProfile(InputDeviceId device, int delta)
    {
        var slot = FindSlotForDevice(device);
        if (slot is not int s)
            return false;
        if (_modes[s] != LobbySlotMode.SelectingProfile)
            return false;
        var state = _profileSelections[s];
        if (state is null)
            return false;
        var available = GetAvailableProfilesForSlot(s);
        state.SyncCarouselIndex(available);
        state.Cycle(delta, available.Count);
        return available.Count > 0;
    }

    public bool TryConfirmProfile(InputDeviceId device)
    {
        var slot = FindSlotForDevice(device);
        if (slot is not int s)
            return false;
        if (_modes[s] != LobbySlotMode.SelectingProfile)
            return false;
        var state = _profileSelections[s];
        if (state is null)
            return false;
        var available = GetAvailableProfilesForSlot(s);
        if (!state.TryConfirm(available))
            return false;
        _modes[s] = LobbySlotMode.SelectingAccessories;
        return true;
    }

    public bool TryReady(InputDeviceId device)
    {
        var slot = FindSlotForDevice(device);
        if (slot is not int s)
            return false;
        if (_modes[s] != LobbySlotMode.SelectingAccessories)
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
                _modes[s] = LobbySlotMode.SelectingAccessories;
                return true;
            case LobbySlotMode.SelectingAccessories:
                _modes[s] = LobbySlotMode.SelectingProfile;
                return true;
            case LobbySlotMode.SelectingProfile:
                _modes[s] = LobbySlotMode.Available;
                _bindings[s].Clear();
                _profileSelections[s] = null;
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
            if (_modes[i] is LobbySlotMode.SelectingProfile
                or LobbySlotMode.SelectingAccessories
                or LobbySlotMode.Ready)
            {
                count = i + 1;
            }
        }

        roster.SetPlayerCount(count);
        for (var i = 0; i < count; i++)
        {
            foreach (var d in _bindings[i].Devices)
                roster.Players[i].AddDevice(d);

            var selection = _accessorySelections[i];
            if (selection is not null)
                roster.Players[i].SetSelectedAccessories(selection.Owned);

            var profileState = _profileSelections[i];
            if (profileState?.ConfirmedProfileId is Guid profileId
                && _profiles.Find(profileId) is { } profile)
            {
                roster.Players[i].SetProfile(profile.Id, profile.Name);
            }
        }

        return roster;
    }

    public void Reset()
    {
        for (var i = 0; i < SlotCount; i++)
        {
            _modes[i] = LobbySlotMode.Available;
            _bindings[i].Clear();
            _profileSelections[i] = null;
            _accessorySelections[i] = null;
        }
    }

    /// <summary>
    /// Hydrate slots from a prior match roster. Lands each restored player in
    /// <see cref="LobbySlotMode.SelectingAccessories"/>. Skips players with no remaining connected devices.
    /// </summary>
    /// <param name="deviceIsConnected">Optional filter; null treats all devices as connected.</param>
    /// <returns>Number of slots restored.</returns>
    public int ApplyFromRoster(
        LocalPlayRoster roster,
        Func<InputDeviceId, bool>? deviceIsConnected = null)
    {
        ArgumentNullException.ThrowIfNull(roster);
        Reset();

        var restored = 0;
        for (var playerIndex = 0; playerIndex < roster.PlayerCount && restored < SlotCount; playerIndex++)
        {
            var entry = roster.Players[playerIndex];
            var devices = new List<InputDeviceId>();
            foreach (var device in entry.Devices)
            {
                if (deviceIsConnected is null || deviceIsConnected(device))
                    devices.Add(device);
            }

            if (devices.Count == 0)
                continue;

            var slot = restored;
            _modes[slot] = LobbySlotMode.SelectingAccessories;
            foreach (var device in devices)
                _bindings[slot].Add(device);

            var profileState = new LobbyProfileSelectionState();
            if (entry.ProfileId is Guid profileId && _profiles.Find(profileId) is not null)
                profileState.RestoreConfirmed(profileId);
            _profileSelections[slot] = profileState;

            var accessoryState = new LobbyAccessorySelectionState(_accessoryPoints);
            foreach (var accessory in entry.SelectedAccessories)
                accessoryState.TryTake(accessory);
            _accessorySelections[slot] = accessoryState;

            restored++;
        }

        return restored;
    }
}
