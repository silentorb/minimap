using Godot;

namespace Minimap.Client.Profiles;

/// <summary>Main-menu Profiles screen: list + detail panel for create/rename/delete/avatar.</summary>
public partial class ProfilesApp : Control
{
    public const string MainMenuScenePath = "res://scenes/main_menu.tscn";
    public const string AchievementsScenePath = "res://scenes/achievements.tscn";
    public const string PlaceholderText = "Select or create a profile";
    public static readonly Vector2 AvatarPreviewSize = new(96, 96);

    private ProfilesScreenModel? _model;
    private string _profilesAbsolutePath = string.Empty;
    private string _avatarsAbsolutePath = string.Empty;
    private LocalPlayContextNode? _playContext;
    private string? _transientError;

    private ItemList? _list;
    private Label? _nameLabel;
    private Label? _deathsLabel;
    private Label? _placeholderLabel;
    private Label? _errorLabel;
    private LineEdit? _nameEdit;
    private Button? _createButton;
    private Button? _renameButton;
    private Button? _deleteButton;
    private Button? _achievementsButton;
    private Button? _changePictureButton;
    private Button? _clearPictureButton;
    private Button? _confirmButton;
    private Button? _cancelButton;
    private Button? _backButton;
    private Control? _detailContent;
    private TextureRect? _avatarPreview;
    private ColorRect? _avatarPlaceholder;
    private FileDialog? _fileDialog;

    public ProfilesScreenModel? Model => _model;

    public override void _Ready()
    {
        _list = GetNode<ItemList>("Margin/HBox/Left/VBox/List");
        _nameLabel = GetNode<Label>("Margin/HBox/Right/VBox/DetailContent/NameLabel");
        _deathsLabel = GetNode<Label>("Margin/HBox/Right/VBox/DetailContent/DeathsLabel");
        _placeholderLabel = GetNode<Label>("Margin/HBox/Right/VBox/PlaceholderLabel");
        _errorLabel = GetNode<Label>("Margin/HBox/Right/VBox/ErrorLabel");
        _nameEdit = GetNode<LineEdit>("Margin/HBox/Right/VBox/NameEdit");
        _createButton = GetNode<Button>("Margin/HBox/Left/VBox/CreateButton");
        _renameButton = GetNode<Button>("Margin/HBox/Right/VBox/DetailContent/ButtonRow/RenameButton");
        _deleteButton = GetNode<Button>("Margin/HBox/Right/VBox/DetailContent/ButtonRow/DeleteButton");
        _achievementsButton = GetNode<Button>("Margin/HBox/Right/VBox/DetailContent/ButtonRow/AchievementsButton");
        _changePictureButton = GetNode<Button>(
            "Margin/HBox/Right/VBox/DetailContent/AvatarRow/AvatarButtonColumn/ChangePictureButton");
        _clearPictureButton = GetNode<Button>(
            "Margin/HBox/Right/VBox/DetailContent/AvatarRow/AvatarButtonColumn/ClearPictureButton");
        _confirmButton = GetNode<Button>("Margin/HBox/Right/VBox/EditButtonRow/ConfirmButton");
        _cancelButton = GetNode<Button>("Margin/HBox/Right/VBox/EditButtonRow/CancelButton");
        _backButton = GetNode<Button>("Margin/HBox/Left/VBox/BackButton");
        _detailContent = GetNode<Control>("Margin/HBox/Right/VBox/DetailContent");
        _avatarPreview = GetNode<TextureRect>("Margin/HBox/Right/VBox/DetailContent/AvatarRow/AvatarPreview");
        _avatarPlaceholder = GetNode<ColorRect>(
            "Margin/HBox/Right/VBox/DetailContent/AvatarRow/AvatarPlaceholder");

        _playContext = GetNode<LocalPlayContextNode>("/root/LocalPlayContext");
        _profilesAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerProfilesResPath);
        _avatarsAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerAvatarsResPath);
        var catalog = WorldHostHooks.RequirePlayerProfiles(_profilesAbsolutePath);
        _model = new ProfilesScreenModel(catalog);

        _fileDialog = new FileDialog
        {
            Name = "AvatarFileDialog",
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Title = "Choose profile picture",
            Exclusive = true,
            UseNativeDialog = true,
        };
        _fileDialog.AddFilter("*.png,*.jpg,*.jpeg,*.webp;Image files");
        AddChild(_fileDialog);
        _fileDialog.FileSelected += OnAvatarFileSelected;

        _list.ItemSelected += OnListItemSelected;
        _createButton.Pressed += OnCreatePressed;
        _renameButton.Pressed += OnRenamePressed;
        _deleteButton.Pressed += OnDeletePressed;
        _achievementsButton.Pressed += OnAchievementsPressed;
        _changePictureButton.Pressed += OnChangePicturePressed;
        _clearPictureButton.Pressed += OnClearPicturePressed;
        _confirmButton.Pressed += OnConfirmPressed;
        _cancelButton.Pressed += OnCancelPressed;
        _backButton.Pressed += OnBackPressed;
        _nameEdit.TextSubmitted += OnNameSubmitted;

        RefreshUi();
        _createButton.GrabFocus();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_model is null)
            return;

        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape)
        {
            if (_model.EditMode != ProfilesScreenEditMode.None)
            {
                _model.CancelEdit();
                RefreshUi();
            }
            else
            {
                OnBackPressed();
            }

            GetViewport().SetInputAsHandled();
        }
    }

    private void OnListItemSelected(long index)
    {
        if (_model is null || _list is null)
            return;
        if (index < 0 || index >= _model.Catalog.Profiles.Count)
            return;
        _transientError = null;
        _model.Select(_model.Catalog.Profiles[(int)index].Id);
        RefreshUi();
    }

    private void OnCreatePressed()
    {
        _transientError = null;
        _model?.BeginCreate();
        RefreshUi();
        _nameEdit?.GrabFocus();
    }

    private void OnRenamePressed()
    {
        _transientError = null;
        _model?.BeginRename();
        RefreshUi();
        _nameEdit?.GrabFocus();
    }

    private void OnDeletePressed()
    {
        _transientError = null;
        _model?.BeginDelete();
        RefreshUi();
        _confirmButton?.GrabFocus();
    }

    private void OnConfirmPressed() => TryConfirm();

    private void OnNameSubmitted(string _) => TryConfirm();

    private void TryConfirm()
    {
        if (_model is null)
            return;
        if (_model.EditMode is ProfilesScreenEditMode.Creating or ProfilesScreenEditMode.Renaming)
            _model.SetEditBuffer(_nameEdit?.Text ?? string.Empty);

        var deleting = _model.EditMode == ProfilesScreenEditMode.ConfirmDelete;
        var avatarToDelete = deleting ? _model.SelectedProfile?.AvatarFile : null;

        if (!_model.ConfirmEdit())
        {
            RefreshUi();
            return;
        }

        Persist();
        if (deleting && !string.IsNullOrWhiteSpace(avatarToDelete))
        {
            var deleteResult = WorldHostHooks.RequireDeletePlayerAvatar(_avatarsAbsolutePath, avatarToDelete);
            if (!deleteResult.Ok)
                _transientError = deleteResult.Error;
        }

        RefreshUi();
    }

    private void OnCancelPressed()
    {
        _transientError = null;
        _model?.CancelEdit();
        RefreshUi();
    }

    private void OnBackPressed() => ChangeSceneOrThrow(MainMenuScenePath);

    private void OnAchievementsPressed()
    {
        if (_model?.SelectedProfile is null || _playContext is null)
            return;
        _playContext.ProfilesFocusId = _model.SelectedProfile.Id;
        ChangeSceneOrThrow(AchievementsScenePath);
    }

    private void OnChangePicturePressed()
    {
        if (_model?.SelectedProfile is null || _fileDialog is null)
            return;
        if (_model.EditMode != ProfilesScreenEditMode.None)
            return;

        _transientError = null;
        _fileDialog.PopupCenteredClamped(new Vector2I(900, 600));
    }

    private void OnClearPicturePressed()
    {
        if (_model?.SelectedProfile is null)
            return;
        if (_model.EditMode != ProfilesScreenEditMode.None)
            return;

        var previous = _model.SelectedProfile.AvatarFile;
        if (string.IsNullOrWhiteSpace(previous))
            return;

        if (!_model.Catalog.TryClearAvatar(_model.SelectedProfile.Id))
        {
            _transientError = "Could not clear avatar.";
            RefreshUi();
            return;
        }

        Persist();
        var deleteResult = WorldHostHooks.RequireDeletePlayerAvatar(_avatarsAbsolutePath, previous);
        _transientError = deleteResult.Ok ? null : deleteResult.Error;
        RefreshUi();
    }

    private void OnAvatarFileSelected(string path)
    {
        if (_model?.SelectedProfile is null)
            return;

        var previous = _model.SelectedProfile.AvatarFile;
        var import = WorldHostHooks.RequireImportPlayerAvatar(
            _avatarsAbsolutePath,
            _model.SelectedProfile.Id,
            path,
            previous);
        if (!import.Ok || string.IsNullOrWhiteSpace(import.AvatarFile))
        {
            _transientError = import.Error ?? "Could not import avatar.";
            RefreshUi();
            return;
        }

        if (!_model.Catalog.TrySetAvatar(_model.SelectedProfile.Id, import.AvatarFile, out var setError))
        {
            _transientError = setError ?? "Could not set avatar.";
            RefreshUi();
            return;
        }

        Persist();
        _transientError = null;
        RefreshUi();
    }

    private void Persist()
    {
        if (_model is null)
            return;
        WorldHostHooks.RequireSavePlayerProfiles(_profilesAbsolutePath, _model.Catalog);
    }

    private void RefreshUi()
    {
        if (_model is null
            || _list is null
            || _nameLabel is null
            || _deathsLabel is null
            || _placeholderLabel is null
            || _errorLabel is null
            || _nameEdit is null
            || _createButton is null
            || _renameButton is null
            || _deleteButton is null
            || _achievementsButton is null
            || _changePictureButton is null
            || _clearPictureButton is null
            || _confirmButton is null
            || _cancelButton is null
            || _detailContent is null
            || _avatarPreview is null
            || _avatarPlaceholder is null)
        {
            return;
        }

        _list.Clear();
        var selectedIndex = -1;
        for (var i = 0; i < _model.Catalog.Profiles.Count; i++)
        {
            var p = _model.Catalog.Profiles[i];
            _list.AddItem($"{p.Name}  ({p.Deaths} deaths)");
            if (_model.SelectedId == p.Id)
                selectedIndex = i;
        }

        if (selectedIndex >= 0)
            _list.Select(selectedIndex);

        var editing = _model.EditMode != ProfilesScreenEditMode.None;
        var selected = _model.SelectedProfile;
        var showDetail = selected is not null && _model.EditMode != ProfilesScreenEditMode.Creating;

        _placeholderLabel.Visible = selected is null && _model.EditMode != ProfilesScreenEditMode.Creating;
        _placeholderLabel.Text = PlaceholderText;
        _detailContent.Visible = showDetail;

        if (selected is not null)
        {
            _nameLabel.Text = selected.Name;
            _deathsLabel.Text = $"Deaths: {selected.Deaths}";
            RefreshAvatarPreview(selected.AvatarFile);
        }
        else
        {
            RefreshAvatarPreview(null);
        }

        _nameEdit.Visible = _model.EditMode is ProfilesScreenEditMode.Creating or ProfilesScreenEditMode.Renaming;
        if (_nameEdit.Visible)
            _nameEdit.Text = _model.EditBuffer;

        _confirmButton.Visible = editing;
        _cancelButton.Visible = editing;
        _confirmButton.Text = _model.EditMode == ProfilesScreenEditMode.ConfirmDelete ? "Delete" : "Confirm";

        var errorText = !string.IsNullOrEmpty(_model.Error) ? _model.Error : _transientError;
        _errorLabel.Visible = !string.IsNullOrEmpty(errorText);
        _errorLabel.Text = errorText ?? string.Empty;

        _createButton.Disabled = editing;
        _renameButton.Disabled = editing || selected is null;
        _deleteButton.Disabled = editing || selected is null;
        _achievementsButton.Disabled = editing || selected is null;
        _changePictureButton.Disabled = editing || selected is null;
        _clearPictureButton.Disabled = editing || selected is null || string.IsNullOrWhiteSpace(selected?.AvatarFile);
        _list.MouseFilter = editing ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;

        if (_model.EditMode == ProfilesScreenEditMode.ConfirmDelete && selected is not null)
        {
            _nameLabel.Text = $"Delete {selected.Name}?";
            _deathsLabel.Text = "This cannot be undone.";
        }
    }

    private void RefreshAvatarPreview(string? avatarFile)
    {
        if (_avatarPreview is null || _avatarPlaceholder is null)
            return;

        var absolute = ProfileAvatarLoader.ResolveAbsolutePath(_avatarsAbsolutePath, avatarFile);
        var texture = ProfileAvatarLoader.TryLoad(absolute);
        if (texture is null)
        {
            _avatarPreview.Texture = null;
            _avatarPreview.Visible = false;
            _avatarPlaceholder.Visible = true;
            _avatarPlaceholder.CustomMinimumSize = AvatarPreviewSize;
            return;
        }

        _avatarPreview.Texture = texture;
        _avatarPreview.CustomMinimumSize = AvatarPreviewSize;
        _avatarPreview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _avatarPreview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _avatarPreview.Visible = true;
        _avatarPlaceholder.Visible = false;
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
