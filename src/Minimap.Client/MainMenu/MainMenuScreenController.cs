namespace Minimap.Client.MainMenu;

/// <summary>Selection state for the full-screen main menu (no exclusive owner).</summary>
public sealed class MainMenuScreenController
{
    private int _selectedIndex;

    public IReadOnlyList<MainMenuAction> Options { get; } = MainMenuModel.ScreenOptions;

    public int SelectedIndex => _selectedIndex;

    public MainMenuAction SelectedAction => Options[_selectedIndex];

    /// <summary>
    /// Applies navigation / activate. Returns true when the event is consumed.
    /// When <paramref name="activated"/> is set, the caller should run that action.
    /// </summary>
    public bool TryHandle(int navigateDelta, bool activateSelected, out MainMenuAction? activated)
    {
        activated = null;

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

    public void Select(MainMenuAction action)
    {
        for (var i = 0; i < Options.Count; i++)
        {
            if (Options[i] != action)
                continue;
            _selectedIndex = i;
            return;
        }
    }
}
