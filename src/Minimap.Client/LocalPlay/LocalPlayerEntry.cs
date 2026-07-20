namespace Minimap.Client.LocalPlay;

/// <summary>One local human player and every device that feeds their input.</summary>
public sealed class LocalPlayerEntry
{
    private readonly HashSet<InputDeviceId> _devices = new();

    public IReadOnlyCollection<InputDeviceId> Devices => _devices;

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
}
