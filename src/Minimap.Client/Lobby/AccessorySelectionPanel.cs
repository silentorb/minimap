using Godot;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>
/// Accessory picker: available grid, description, owned grid.
/// Interactive only while the parent lobby panel is Claimed.
/// </summary>
public partial class AccessorySelectionPanel : VBoxContainer
{
    private const int GridColumns = 4;
    private const int IconSize = 48;

    private Label? _pointsLabel;
    private Label? _descriptionLabel;
    private GridContainer? _availableGrid;
    private GridContainer? _ownedGrid;
    private LobbyAccessorySelectionState? _state;
    private IReadOnlyList<AccessoryDefinition> _catalog = Array.Empty<AccessoryDefinition>();
    private readonly List<AccessoryDefinition> _availableOrder = new();
    private readonly List<Button> _availableButtons = new();
    private readonly List<Button> _ownedButtons = new();
    private int _focusIndex;
    private bool _focusOwned;
    private bool _interactive;

    public int AccessoryPointsBudget { get; private set; }

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 6);

        _pointsLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        AddChild(_pointsLabel);

        AddChild(new Label { Text = "Available", HorizontalAlignment = HorizontalAlignment.Center });
        _availableGrid = new GridContainer { Columns = GridColumns };
        AddChild(_availableGrid);

        _descriptionLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, 56),
        };
        AddChild(_descriptionLabel);

        AddChild(new Label { Text = "Owned", HorizontalAlignment = HorizontalAlignment.Center });
        _ownedGrid = new GridContainer { Columns = GridColumns };
        AddChild(_ownedGrid);
    }

    public void Configure(
        IReadOnlyList<AccessoryDefinition> catalog,
        LobbyAccessorySelectionState state,
        bool interactive)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(state);
        _catalog = catalog;
        _state = state;
        AccessoryPointsBudget = state.AccessoryPoints;
        _interactive = interactive;
        Visible = true;
        Rebuild();
        if (!interactive)
            ClearFocusVisual();
        else
            EnsureFocus();
    }

    public void HideSelection()
    {
        Visible = false;
        _interactive = false;
        _state = null;
    }

    public LobbyAccessorySelectionState? State => _state;

    public bool HandleNavigate(Vector2I delta)
    {
        if (!_interactive || _state is null)
            return false;

        var buttons = _focusOwned ? _ownedButtons : _availableButtons;
        if (buttons.Count == 0)
        {
            if (!_focusOwned && _ownedButtons.Count > 0)
            {
                _focusOwned = true;
                _focusIndex = 0;
                RefreshFocusVisual();
                return true;
            }

            if (_focusOwned && _availableButtons.Count > 0)
            {
                _focusOwned = false;
                _focusIndex = 0;
                RefreshFocusVisual();
                return true;
            }

            return false;
        }

        if (delta.Y != 0 && ((_focusOwned && delta.Y < 0) || (!_focusOwned && delta.Y > 0)))
        {
            // Switch lists vertically when possible.
            if (!_focusOwned && _ownedButtons.Count > 0 && delta.Y > 0)
            {
                _focusOwned = true;
                _focusIndex = Math.Min(_focusIndex, _ownedButtons.Count - 1);
                RefreshFocusVisual();
                return true;
            }

            if (_focusOwned && _availableButtons.Count > 0 && delta.Y < 0)
            {
                _focusOwned = false;
                _focusIndex = Math.Min(_focusIndex, _availableButtons.Count - 1);
                RefreshFocusVisual();
                return true;
            }
        }

        var cols = GridColumns;
        var count = buttons.Count;
        var row = _focusIndex / cols;
        var col = _focusIndex % cols;
        row = Math.Clamp(row + delta.Y, 0, (count - 1) / cols);
        col = Math.Clamp(col + delta.X, 0, cols - 1);
        var next = row * cols + col;
        if (next >= count)
            next = count - 1;
        if (next == _focusIndex && delta == Vector2I.Zero)
            return false;

        _focusIndex = next;
        RefreshFocusVisual();
        return true;
    }

    public bool HandleActivate()
    {
        if (!_interactive || _state is null)
            return false;

        if (_focusOwned)
        {
            if (_focusIndex < 0 || _focusIndex >= _state.Owned.Count)
                return false;
            var accessory = _state.Owned[_focusIndex];
            if (!_state.TryReturn(accessory))
                return false;
            Rebuild();
            EnsureFocus();
            return true;
        }

        if (_focusIndex < 0 || _focusIndex >= _availableOrder.Count)
            return false;
        var take = _availableOrder[_focusIndex];
        if (!_state.TryTake(take))
            return false;
        Rebuild();
        EnsureFocus();
        return true;
    }

    private void Rebuild()
    {
        if (_state is null || _pointsLabel is null || _descriptionLabel is null
            || _availableGrid is null || _ownedGrid is null)
            return;

        _pointsLabel.Text = $"Accessory points: {_state.RemainingPoints}/{_state.AccessoryPoints}";

        var ownedIds = _state.Owned.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        _availableOrder.Clear();
        foreach (var accessory in _catalog)
        {
            if (!ownedIds.Contains(accessory.Id))
                _availableOrder.Add(accessory);
        }

        RebuildGrid(_availableGrid, _availableButtons, _availableOrder, owned: false);
        RebuildGrid(_ownedGrid, _ownedButtons, _state.Owned, owned: true);
        UpdateDescription();
        RefreshFocusVisual();
    }

    private void RebuildGrid(
        GridContainer grid,
        List<Button> buttons,
        IReadOnlyList<AccessoryDefinition> items,
        bool owned)
    {
        foreach (var child in grid.GetChildren())
            child.QueueFree();
        buttons.Clear();

        for (var i = 0; i < items.Count; i++)
        {
            var accessory = items[i];
            var index = i;
            var button = new Button
            {
                CustomMinimumSize = new Vector2(IconSize + 8, IconSize + 8),
                FocusMode = _interactive ? FocusModeEnum.All : FocusModeEnum.None,
                Disabled = !_interactive,
                TooltipText = accessory.DisplayName ?? accessory.Id,
            };

            var icon = TryLoadIcon(accessory);
            if (icon is not null)
                button.Icon = icon;
            else
                button.Text = (accessory.DisplayName ?? accessory.Id).Length > 0
                    ? (accessory.DisplayName ?? accessory.Id)[..1].ToUpperInvariant()
                    : "?";

            if (_interactive)
            {
                var capturedOwned = owned;
                button.Pressed += () =>
                {
                    _focusOwned = capturedOwned;
                    _focusIndex = index;
                    HandleActivate();
                };
            }

            grid.AddChild(button);
            buttons.Add(button);
        }
    }

    private static Texture2D? TryLoadIcon(AccessoryDefinition accessory)
    {
        var path = accessory.IconConfig?.ResourcePath;
        if (string.IsNullOrWhiteSpace(path))
            return null;
        if (!ResourceLoader.Exists(path))
            return null;
        return ResourceLoader.Load<Texture2D>(path);
    }

    private void EnsureFocus()
    {
        if (!_interactive)
            return;

        if (!_focusOwned && _availableButtons.Count == 0 && _ownedButtons.Count > 0)
            _focusOwned = true;
        if (_focusOwned && _ownedButtons.Count == 0 && _availableButtons.Count > 0)
            _focusOwned = false;

        var count = _focusOwned ? _ownedButtons.Count : _availableButtons.Count;
        if (count == 0)
        {
            _focusIndex = 0;
            UpdateDescription();
            return;
        }

        _focusIndex = Math.Clamp(_focusIndex, 0, count - 1);
        RefreshFocusVisual();
    }

    private void RefreshFocusVisual()
    {
        foreach (var button in _availableButtons)
            button.Modulate = Colors.White;
        foreach (var button in _ownedButtons)
            button.Modulate = Colors.White;

        var buttons = _focusOwned ? _ownedButtons : _availableButtons;
        if (_interactive && _focusIndex >= 0 && _focusIndex < buttons.Count)
            buttons[_focusIndex].Modulate = new Color(1.2f, 1.2f, 0.7f);

        UpdateDescription();
    }

    private void ClearFocusVisual()
    {
        foreach (var button in _availableButtons)
            button.Modulate = Colors.White;
        foreach (var button in _ownedButtons)
            button.Modulate = Colors.White;
        if (_descriptionLabel is not null)
            _descriptionLabel.Text = string.Empty;
    }

    private void UpdateDescription()
    {
        if (_descriptionLabel is null || _state is null)
            return;

        AccessoryDefinition? focused = null;
        if (_focusOwned)
        {
            if (_focusIndex >= 0 && _focusIndex < _state.Owned.Count)
                focused = _state.Owned[_focusIndex];
        }
        else if (_focusIndex >= 0 && _focusIndex < _availableOrder.Count)
        {
            focused = _availableOrder[_focusIndex];
        }

        if (focused is null)
        {
            _descriptionLabel.Text = string.Empty;
            return;
        }

        var name = focused.DisplayName ?? focused.Id;
        var cost = focused.PointCost;
        var body = focused.Description ?? string.Empty;
        _descriptionLabel.Text = string.IsNullOrWhiteSpace(body)
            ? $"{name} (cost {cost})"
            : $"{name} (cost {cost})\n{body}";
    }
}
