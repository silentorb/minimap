using Godot;
using Minimap.Client.Achievements;

namespace Minimap.Client.Profiles;

/// <summary>Full-screen achievements list for one profile (from Profiles).</summary>
public partial class AchievementsApp : Control
{
    public const string ProfilesScenePath = "res://scenes/profiles.tscn";

    private ItemList? _list;
    private Label? _titleLabel;
    private Button? _backButton;
    private LocalPlayContextNode? _playContext;

    public override void _Ready()
    {
        _list = GetNode<ItemList>("Margin/VBox/List");
        _titleLabel = GetNode<Label>("Margin/VBox/Title");
        _backButton = GetNode<Button>("Margin/VBox/BackButton");
        _backButton.Pressed += OnBackPressed;

        _playContext = GetNode<LocalPlayContextNode>("/root/LocalPlayContext");
        var profileId = _playContext.ProfilesFocusId;
        if (profileId is null)
        {
            OnBackPressed();
            return;
        }

        var path = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerProfilesResPath);
        var catalog = WorldHostHooks.RequirePlayerProfiles(path);
        var profile = catalog.Find(profileId.Value);
        if (profile is null)
        {
            OnBackPressed();
            return;
        }

        if (_titleLabel is not null)
            _titleLabel.Text = $"Achievements — {profile.Name}";

        if (_list is not null)
        {
            foreach (var def in AchievementCatalog.Definitions)
            {
                var unlocked = profile.HasAchievement(def.Id);
                var status = unlocked ? "Unlocked" : "Locked";
                _list.AddItem($"{def.Title}  [{status}]");
            }
        }

        _backButton.GrabFocus();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape)
        {
            OnBackPressed();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnBackPressed()
    {
        if (_playContext is not null)
            _playContext.ProfilesFocusId = null;
        ChangeSceneOrThrow(ProfilesScenePath);
    }

    private void ChangeSceneOrThrow(string path)
    {
        var error = GetTree().ChangeSceneToFile(path);
        if (error == Error.Ok)
            return;

        GD.PushError($"ChangeSceneToFile failed for '{path}': {error}");
        throw new InvalidOperationException($"ChangeSceneToFile failed for '{path}' with {error}.");
    }
}
