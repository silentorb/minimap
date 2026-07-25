using Minimap.Client.LocalPlay;

namespace Minimap.Client.Lobby;

/// <summary>Devices bound to one lobby slot while SelectingProfile, SelectingAccessories, or Ready.</summary>
public sealed class LobbySlotBinding
{
    private readonly HashSet<InputDeviceId> _devices = new();

    public IReadOnlyCollection<InputDeviceId> Devices => _devices;

    public void Add(InputDeviceId device) => _devices.Add(device);

    public void Clear() => _devices.Clear();

    public bool Contains(InputDeviceId device) => _devices.Contains(device);
}
