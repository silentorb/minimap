using Godot;
using Minimap.Client.Profiles;

namespace Minimap.Client.Lobby;

/// <summary>Select-only profile carousel for a lobby slot customize area.</summary>
public partial class ProfileSelectionPanel : Control
{
    public const string EmptyHint =
        "No profiles available.\nCreate one from the main menu Profiles screen.";

    public static readonly Vector2 AvatarSize = new(64, 64);

    private Label? _titleLabel;
    private TextureRect? _avatar;
    private ColorRect? _avatarPlaceholder;
    private Label? _bodyLabel;
    private Label? _hintLabel;
    private string _avatarsAbsolutePath = string.Empty;

    public override void _Ready()
    {
        _avatarsAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerAvatarsResPath);

        var vbox = new VBoxContainer
        {
            Name = "VBox",
        };
        vbox.SetAnchorsPreset(LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 8);
        AddChild(vbox);

        _titleLabel = new Label
        {
            Name = "Title",
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "Select profile",
        };
        vbox.AddChild(_titleLabel);

        var avatarRow = new HBoxContainer
        {
            Name = "AvatarRow",
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        vbox.AddChild(avatarRow);

        _avatarPlaceholder = new ColorRect
        {
            Name = "AvatarPlaceholder",
            CustomMinimumSize = AvatarSize,
            Color = new Color(0.22f, 0.24f, 0.28f),
        };
        avatarRow.AddChild(_avatarPlaceholder);

        _avatar = new TextureRect
        {
            Name = "Avatar",
            Visible = false,
            CustomMinimumSize = AvatarSize,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        avatarRow.AddChild(_avatar);

        _bodyLabel = new Label
        {
            Name = "Body",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        vbox.AddChild(_bodyLabel);

        _hintLabel = new Label
        {
            Name = "Hint",
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "← → cycle   Enter / Start confirm",
        };
        vbox.AddChild(_hintLabel);
    }

    public void ShowEmpty()
    {
        Visible = true;
        if (_bodyLabel is not null)
            _bodyLabel.Text = EmptyHint;
        if (_hintLabel is not null)
            _hintLabel.Visible = false;
        ApplyAvatar(null);
    }

    public void ShowProfile(PlayerProfileRecord profile, int index, int count)
    {
        Visible = true;
        if (_bodyLabel is not null)
        {
            _bodyLabel.Text = $"{profile.Name}\nDeaths: {profile.Deaths}\n({index + 1} / {count})";
        }

        if (_hintLabel is not null)
            _hintLabel.Visible = true;
        ApplyAvatar(profile.AvatarFile);
    }

    public void HideSelection() => Visible = false;

    private void ApplyAvatar(string? avatarFile)
    {
        if (_avatar is null || _avatarPlaceholder is null)
            return;

        var absolute = ProfileAvatarLoader.ResolveAbsolutePath(_avatarsAbsolutePath, avatarFile);
        var texture = ProfileAvatarLoader.TryLoad(absolute);
        if (texture is null)
        {
            _avatar.Texture = null;
            _avatar.Visible = false;
            _avatarPlaceholder.Visible = true;
            return;
        }

        _avatar.Texture = texture;
        _avatar.Visible = true;
        _avatarPlaceholder.Visible = false;
    }
}
