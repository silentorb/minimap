using Godot;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>
/// Accessory picker: Available / Description / Owned panels that share parent height.
/// Interactive only while the parent lobby panel is Claimed.
/// </summary>
public partial class AccessorySelectionPanel : VBoxContainer
{
    private const int MinColumns = 2;
    private const int MaxColumns = 4;
    private const int MinIconSize = 24;
    private const int MaxIconSize = 48;
    private const int IconPadding = 8;
    private const int GridSeparation = 4;

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
    private int _gridColumns = MaxColumns;
    private int _iconSize = MaxIconSize;

    public int AccessoryPointsBudget { get; private set; }

    public override void _Ready()
    {
        AddThemeConstantOverride("separation", 4);

        _pointsLabel = new Label
        {
            Name = "Points",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        AddChild(_pointsLabel);

        var availablePanel = CreateSectionPanel("Available", withHeader: true, out var availableScroll);
        _availableGrid = new GridContainer
        {
            Name = "Grid",
            Columns = _gridColumns,
        };
        _availableGrid.AddThemeConstantOverride("h_separation", GridSeparation);
        _availableGrid.AddThemeConstantOverride("v_separation", GridSeparation);
        availableScroll.AddChild(_availableGrid);
        AddChild(availablePanel);

        var descriptionPanel = CreateSectionPanel("Description", withHeader: false, out var descriptionScroll);
        _descriptionLabel = new Label
        {
            Name = "Body",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        descriptionScroll.AddChild(_descriptionLabel);
        AddChild(descriptionPanel);

        var ownedPanel = CreateSectionPanel("Owned", withHeader: true, out var ownedScroll);
        _ownedGrid = new GridContainer
        {
            Name = "Grid",
            Columns = _gridColumns,
        };
        _ownedGrid.AddThemeConstantOverride("h_separation", GridSeparation);
        _ownedGrid.AddThemeConstantOverride("v_separation", GridSeparation);
        ownedScroll.AddChild(_ownedGrid);
        AddChild(ownedPanel);
    }

    /// <summary>Scale columns and icon size so grids fit <paramref name="availableSize"/> width.</summary>
    public void Relayout(Vector2 availableSize)
    {
        if (availableSize.X <= 1f)
            return;

        var width = availableSize.X;
        var columns = MaxColumns;
        var iconSize = MaxIconSize;
        for (var tryColumns = MaxColumns; tryColumns >= MinColumns; tryColumns--)
        {
            var separations = GridSeparation * Math.Max(0, tryColumns - 1);
            var cell = (int)Math.Floor((width - separations) / tryColumns);
            var candidateIcon = cell - IconPadding;
            if (candidateIcon >= MinIconSize)
            {
                columns = tryColumns;
                iconSize = Math.Clamp(candidateIcon, MinIconSize, MaxIconSize);
                break;
            }

            if (tryColumns == MinColumns)
            {
                columns = MinColumns;
                iconSize = MinIconSize;
            }
        }

        if (columns == _gridColumns && iconSize == _iconSize)
            return;

        _gridColumns = columns;
        _iconSize = iconSize;
        if (_availableGrid is not null)
            _availableGrid.Columns = _gridColumns;
        if (_ownedGrid is not null)
            _ownedGrid.Columns = _gridColumns;

        if (_state is not null)
            Rebuild();
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

        var cols = _gridColumns;
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

    private static PanelContainer CreateSectionPanel(string name, bool withHeader, out ScrollContainer scroll)
    {
        var panel = new PanelContainer
        {
            Name = name,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.12f, 0.13f, 0.16f, 0.65f),
            ContentMarginLeft = 4,
            ContentMarginTop = 4,
            ContentMarginRight = 4,
            ContentMarginBottom = 4,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
        });

        var vbox = new VBoxContainer
        {
            Name = "Content",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        vbox.AddThemeConstantOverride("separation", 2);
        panel.AddChild(vbox);

        if (withHeader)
        {
            vbox.AddChild(new Label
            {
                Name = "Header",
                Text = name,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
        }

        scroll = new ScrollContainer
        {
            Name = "Scroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
        };
        vbox.AddChild(scroll);
        return panel;
    }

    private void Rebuild()
    {
        if (_state is null || _pointsLabel is null || _descriptionLabel is null
            || _availableGrid is null || _ownedGrid is null)
            return;

        _pointsLabel.Text = $"Points available: {_state.RemainingPoints}";

        var ownedIds = _state.Owned.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        _availableOrder.Clear();
        foreach (var accessory in _catalog)
        {
            if (!ownedIds.Contains(accessory.Id))
                _availableOrder.Add(accessory);
        }

        RebuildGrid(_availableGrid, _availableButtons, _availableOrder, owned: false);
        RebuildOwnedGrid();
        UpdateDescription();
        RefreshFocusVisual();
    }

    private void RebuildOwnedGrid()
    {
        if (_state is null || _ownedGrid is null)
            return;

        foreach (var child in _ownedGrid.GetChildren())
            child.QueueFree();
        _ownedButtons.Clear();

        for (var i = 0; i < _state.Owned.Count; i++)
        {
            var accessory = _state.Owned[i];
            var index = i;
            var locked = _state.IsLocked(accessory);
            var button = CreateIconButton(accessory, locked);
            if (_interactive)
            {
                button.Pressed += () =>
                {
                    _focusOwned = true;
                    _focusIndex = index;
                    HandleActivate();
                };
            }

            if (locked)
                button.Modulate = new Color(0.65f, 0.65f, 0.7f);

            _ownedGrid.AddChild(button);
            _ownedButtons.Add(button);
        }
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
            var button = CreateIconButton(accessory, locked: false);
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

    private Button CreateIconButton(AccessoryDefinition accessory, bool locked)
    {
        var button = new Button
        {
            CustomMinimumSize = new Vector2(_iconSize + IconPadding, _iconSize + IconPadding),
            FocusMode = _interactive ? FocusModeEnum.All : FocusModeEnum.None,
            Disabled = !_interactive,
            TooltipText = locked
                ? $"{accessory.DisplayName ?? accessory.Id} (locked)"
                : accessory.DisplayName ?? accessory.Id,
            ExpandIcon = true,
        };

        var icon = TryLoadIcon(accessory);
        if (icon is not null)
            button.Icon = icon;
        else
            button.Text = (accessory.DisplayName ?? accessory.Id).Length > 0
                ? (accessory.DisplayName ?? accessory.Id)[..1].ToUpperInvariant()
                : "?";

        return button;
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

        if (_state is not null)
        {
            for (var i = 0; i < _ownedButtons.Count && i < _state.Owned.Count; i++)
            {
                _ownedButtons[i].Modulate = _state.IsLocked(_state.Owned[i])
                    ? new Color(0.65f, 0.65f, 0.7f)
                    : Colors.White;
            }
        }

        var buttons = _focusOwned ? _ownedButtons : _availableButtons;
        if (_interactive && _focusIndex >= 0 && _focusIndex < buttons.Count)
            buttons[_focusIndex].Modulate = new Color(1.2f, 1.2f, 0.7f);

        UpdateDescription();
    }

    private void ClearFocusVisual()
    {
        foreach (var button in _availableButtons)
            button.Modulate = Colors.White;
        if (_state is not null)
        {
            for (var i = 0; i < _ownedButtons.Count && i < _state.Owned.Count; i++)
            {
                _ownedButtons[i].Modulate = _state.IsLocked(_state.Owned[i])
                    ? new Color(0.65f, 0.65f, 0.7f)
                    : Colors.White;
            }
        }

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
        var locked = _focusOwned && _state.IsLocked(focused);
        var header = locked
            ? $"{name} (locked — cannot unchoose)"
            : $"{name} (cost {cost})";
        _descriptionLabel.Text = string.IsNullOrWhiteSpace(body)
            ? header
            : $"{header}\n{body}";
    }
}
