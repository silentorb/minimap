using Minimap.Client.LocalPlay;

namespace Minimap.Client.MainMenu;

/// <summary>Tracks exclusive device ownership while the main menu popup is open.</summary>
public sealed class MainMenuOwnership
{
    public InputDeviceId? Owner { get; private set; }

    public bool IsOpen => Owner is not null;

    public void Open(InputDeviceId device) => Owner = device;

    public void Close() => Owner = null;

    public bool Accepts(InputDeviceId device) =>
        Owner is InputDeviceId owner && owner.Equals(device);
}
