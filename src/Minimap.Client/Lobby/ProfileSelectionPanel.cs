using Godot;
using Minimap.Client.Profiles;

namespace Minimap.Client.Lobby;

/// <summary>Select-only profile carousel for a lobby slot customize area.</summary>
public partial class ProfileSelectionPanel : Control
{
    public const string EmptyHint =
        "No profiles available.\nCreate one from the main menu Profiles screen.";

    private Label? _titleLabel;
    private Label? _bodyLabel;
    private Label? _hintLabel;

    public override void _Ready()
    {
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
    }

    public void HideSelection() => Visible = false;
}
