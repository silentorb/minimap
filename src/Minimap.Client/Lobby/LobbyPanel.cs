using Godot;
using Minimap.Client.Profiles;
using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>One of four lobby player panels with profile carousel and accessory selection.</summary>
public partial class LobbyPanel : PanelContainer
{
    public static readonly Vector2 TitleAvatarSize = new(28, 28);

    private Label? _title;
    private TextureRect? _titleAvatar;
    private Label? _status;
    private Control? _customizeArea;
    private Control? _navRow;
    private Button? _backButton;
    private Button? _forwardButton;
    private AccessorySelectionPanel? _accessoryPanel;
    private ProfileSelectionPanel? _profilePanel;
    private string _avatarsAbsolutePath = string.Empty;

    public AccessorySelectionPanel? AccessoryPanel => _accessoryPanel;

    public ProfileSelectionPanel? ProfilePanel => _profilePanel;

    /// <summary>Raised when the step Back button is pressed.</summary>
    public event Action? BackPressed;

    /// <summary>Raised when the step Forward button is pressed.</summary>
    public event Action? ForwardPressed;

    public override void _Ready()
    {
        _avatarsAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerAvatarsResPath);
        _title = GetNode<Label>("Margin/VBox/TitleRow/Title");
        _titleAvatar = GetNode<TextureRect>("Margin/VBox/TitleRow/TitleAvatar");
        _status = GetNode<Label>("Margin/VBox/Status");
        _customizeArea = GetNode<Control>("Margin/VBox/CustomizeArea");
        _navRow = GetNode<Control>("Margin/VBox/NavRow");
        _backButton = GetNode<Button>("Margin/VBox/NavRow/BackButton");
        _forwardButton = GetNode<Button>("Margin/VBox/NavRow/ForwardButton");

        _backButton.Pressed += () => BackPressed?.Invoke();
        _forwardButton.Pressed += () => ForwardPressed?.Invoke();

        _accessoryPanel = new AccessorySelectionPanel
        {
            Name = "AccessorySelection",
            Visible = false,
        };
        _accessoryPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _accessoryPanel.OffsetLeft = 0;
        _accessoryPanel.OffsetTop = 0;
        _accessoryPanel.OffsetRight = 0;
        _accessoryPanel.OffsetBottom = 0;
        _customizeArea!.AddChild(_accessoryPanel);

        _profilePanel = new ProfileSelectionPanel
        {
            Name = "ProfileSelection",
            Visible = false,
        };
        _profilePanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _profilePanel.OffsetLeft = 0;
        _profilePanel.OffsetTop = 0;
        _profilePanel.OffsetRight = 0;
        _profilePanel.OffsetBottom = 0;
        _customizeArea.AddChild(_profilePanel);

        _customizeArea.Resized += OnCustomizeAreaResized;

        ApplyMode(LobbySlotMode.Available, 0);
    }

    public override void _ExitTree()
    {
        if (_customizeArea is not null)
            _customizeArea.Resized -= OnCustomizeAreaResized;
        base._ExitTree();
    }

    public void ApplyMode(
        LobbySlotMode mode,
        int slotIndex,
        string? profileDisplayName = null,
        bool canAdvance = true,
        string? avatarFile = null)
    {
        if (_title is null || _status is null)
            return;

        _title.Text = string.IsNullOrWhiteSpace(profileDisplayName)
            ? $"Player {slotIndex + 1}"
            : profileDisplayName;
        ApplyTitleAvatar(string.IsNullOrWhiteSpace(profileDisplayName) ? null : avatarFile);
        _status.Text = mode switch
        {
            LobbySlotMode.Available => "Available",
            LobbySlotMode.SelectingProfile or LobbySlotMode.SelectingAccessories => "Joined",
            LobbySlotMode.Ready => "Ready",
            _ => "Available",
        };

        var baseColor = mode switch
        {
            LobbySlotMode.Available => new Color(0.15f, 0.16f, 0.2f),
            LobbySlotMode.SelectingProfile or LobbySlotMode.SelectingAccessories =>
                new Color(0.18f, 0.28f, 0.38f),
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

        ApplyStepNav(mode, canAdvance);
    }

    private void ApplyTitleAvatar(string? avatarFile)
    {
        if (_titleAvatar is null)
            return;

        var absolute = ProfileAvatarLoader.ResolveAbsolutePath(_avatarsAbsolutePath, avatarFile);
        var texture = ProfileAvatarLoader.TryLoad(absolute);
        if (texture is null)
        {
            _titleAvatar.Texture = null;
            _titleAvatar.Visible = false;
            return;
        }

        _titleAvatar.Texture = texture;
        _titleAvatar.CustomMinimumSize = TitleAvatarSize;
        _titleAvatar.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _titleAvatar.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _titleAvatar.Visible = true;
    }

    private void ApplyStepNav(LobbySlotMode mode, bool canAdvance)
    {
        if (_navRow is null || _backButton is null || _forwardButton is null)
            return;

        var canBack = LobbyStepNavigation.CanGoBack(mode);
        var canForward = LobbyStepNavigation.CanGoForward(mode);
        _navRow.Visible = canBack || canForward;
        _backButton.Visible = canBack;
        _forwardButton.Visible = canForward;
        _forwardButton.Disabled = canForward && !canAdvance;
    }

    public void ShowProfileSelection(IReadOnlyList<PlayerProfileRecord> available, LobbyProfileSelectionState state)
    {
        if (_profilePanel is null)
            return;
        HideAccessorySelection();
        state.SyncCarouselIndex(available);
        if (available.Count == 0)
        {
            _profilePanel.ShowEmpty();
            return;
        }

        _profilePanel.ShowProfile(available[state.CarouselIndex], state.CarouselIndex, available.Count);
    }

    public void HideProfileSelection()
    {
        _profilePanel?.HideSelection();
    }

    public void ShowAccessorySelection(
        IReadOnlyList<AccessoryDefinition> catalog,
        LobbyAccessorySelectionState state,
        bool interactive,
        IReadOnlyList<DomainDefinition>? domains = null)
    {
        if (_accessoryPanel is null || _customizeArea is null)
            return;
        HideProfileSelection();
        _accessoryPanel.Configure(catalog, state, interactive, domains);
        _accessoryPanel.Visible = interactive;
        CallDeferred(nameof(RelayoutAccessorySelection));
    }

    public void HideAccessorySelection()
    {
        _accessoryPanel?.HideSelection();
    }

    private void OnCustomizeAreaResized() => RelayoutAccessorySelection();

    private void RelayoutAccessorySelection()
    {
        if (_accessoryPanel is null || _customizeArea is null || !_accessoryPanel.Visible)
            return;
        _accessoryPanel.Relayout(_customizeArea.Size);
    }
}
