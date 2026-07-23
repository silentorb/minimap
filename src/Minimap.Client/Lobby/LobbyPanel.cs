using Godot;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>One of four lobby player panels with optional accessory selection.</summary>
public partial class LobbyPanel : PanelContainer
{
    private Label? _title;
    private Label? _status;
    private Control? _customizeArea;
    private AccessorySelectionPanel? _accessoryPanel;

    public AccessorySelectionPanel? AccessoryPanel => _accessoryPanel;

    public override void _Ready()
    {
        _title = GetNode<Label>("Margin/VBox/Title");
        _status = GetNode<Label>("Margin/VBox/Status");
        _customizeArea = GetNode<Control>("Margin/VBox/CustomizeArea");

        _accessoryPanel = new AccessorySelectionPanel
        {
            Name = "AccessorySelection",
            Visible = false,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        _customizeArea!.AddChild(_accessoryPanel);

        ApplyMode(LobbySlotMode.Available, 0);
    }

    public void ApplyMode(LobbySlotMode mode, int slotIndex)
    {
        if (_title is null || _status is null)
            return;

        _title.Text = $"Player {slotIndex + 1}";
        _status.Text = mode switch
        {
            LobbySlotMode.Available => "Available",
            LobbySlotMode.Claimed => "Joined",
            LobbySlotMode.Ready => "Ready",
            _ => "Available",
        };

        var baseColor = mode switch
        {
            LobbySlotMode.Available => new Color(0.15f, 0.16f, 0.2f),
            LobbySlotMode.Claimed => new Color(0.18f, 0.28f, 0.38f),
            LobbySlotMode.Ready => new Color(0.15f, 0.32f, 0.22f),
            _ => new Color(0.15f, 0.16f, 0.2f),
        };
        AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = baseColor,
            BorderColor = new Color(0.35f, 0.38f, 0.45f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ContentMarginLeft = 8,
            ContentMarginTop = 8,
            ContentMarginRight = 8,
            ContentMarginBottom = 8,
        });
    }

    public void ShowAccessorySelection(
        IReadOnlyList<AccessoryDefinition> catalog,
        LobbyAccessorySelectionState state,
        bool interactive)
    {
        if (_accessoryPanel is null)
            return;
        _accessoryPanel.Configure(catalog, state, interactive);
        _accessoryPanel.Visible = interactive;
    }

    public void HideAccessorySelection()
    {
        _accessoryPanel?.HideSelection();
    }
}
