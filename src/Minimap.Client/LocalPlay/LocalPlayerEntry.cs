using Minimap.Simulation.Types;

namespace Minimap.Client.LocalPlay;

/// <summary>One local human player and every device that feeds their input.</summary>
public sealed class LocalPlayerEntry
{
    private readonly HashSet<InputDeviceId> _devices = new();
    private readonly List<AccessoryDefinition> _selectedAccessories = new();

    public IReadOnlyCollection<InputDeviceId> Devices => _devices;

    public IReadOnlyList<AccessoryDefinition> SelectedAccessories => _selectedAccessories;

    public Guid? ProfileId { get; private set; }

    public string? DisplayName { get; private set; }

    /// <summary>Relative avatar filename under the profile avatars directory, when set.</summary>
    public string? AvatarFile { get; private set; }

    public void SetProfile(Guid profileId, string displayName, string? avatarFile = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ProfileId = profileId;
        DisplayName = displayName;
        AvatarFile = string.IsNullOrWhiteSpace(avatarFile) ? null : avatarFile.Trim();
    }

    public void ClearProfile()
    {
        ProfileId = null;
        DisplayName = null;
        AvatarFile = null;
    }

    public void AddDevice(InputDeviceId device) => _devices.Add(device);

    public void RemoveDevice(InputDeviceId device) => _devices.Remove(device);

    public void ClearDevices() => _devices.Clear();

    public bool HasDevice(InputDeviceId device) => _devices.Contains(device);

    public bool HasJoypad(int deviceIndex) =>
        _devices.Any(d => d.IsJoypad && d.JoypadDevice == deviceIndex);

    public bool HasKeyboard => _devices.Any(d => d.IsKeyboard);

    public void ReplaceJoypad(int oldDeviceIndex, int newDeviceIndex)
    {
        _devices.Remove(InputDeviceId.Joypad(oldDeviceIndex));
        _devices.Add(InputDeviceId.Joypad(newDeviceIndex));
    }

    public void SetSelectedAccessories(IEnumerable<AccessoryDefinition> accessories)
    {
        ArgumentNullException.ThrowIfNull(accessories);
        _selectedAccessories.Clear();
        _selectedAccessories.AddRange(accessories);
    }

    public void ClearSelectedAccessories() => _selectedAccessories.Clear();
}
