using Minimap.Client.LocalPlay;

namespace Minimap.Client.MainMenu;

/// <summary>Exclusive owner selection state for the in-world main menu popup.</summary>
public sealed class MainMenuPopupController
{
    private readonly MainMenuOwnership _ownership = new();
    private int _selectedIndex;

    public MainMenuOwnership Ownership => _ownership;

    public IReadOnlyList<MainMenuAction> Options { get; } = MainMenuModel.PopupOptions(activeGame: true);

    public int SelectedIndex => _selectedIndex;

    public MainMenuAction SelectedAction => Options[_selectedIndex];

    public bool IsOpen => _ownership.IsOpen;

    public void Open(InputDeviceId owner)
    {
        _ownership.Open(owner);
        _selectedIndex = 0;
    }

    public void Close()
    {
        _ownership.Close();
        _selectedIndex = 0;
    }

    /// <summary>
    /// Applies owner input. Returns true when the event is consumed.
    /// When <paramref name="activated"/> is set, the caller should run that action.
    /// </summary>
    public bool TryHandle(
        InputDeviceId device,
        int navigateDelta,
        bool activateSelected,
        bool dismiss,
        out MainMenuAction? activated)
    {
        activated = null;
        if (!_ownership.Accepts(device))
            return false;

        if (dismiss)
        {
            activated = MainMenuAction.Continue;
            return true;
        }

        if (navigateDelta != 0)
        {
            var count = Options.Count;
            _selectedIndex = ((_selectedIndex + navigateDelta) % count + count) % count;
            return true;
        }

        if (activateSelected)
        {
            activated = SelectedAction;
            return true;
        }

        return false;
    }
}
