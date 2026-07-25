using Godot;

namespace Minimap.Client.Profiles;

/// <summary>Main-menu Profiles screen: list + detail panel for create/rename/delete.</summary>
public partial class ProfilesApp : Control
{
    public const string MainMenuScenePath = "res://scenes/main_menu.tscn";
    public const string PlaceholderText = "Select or create a profile";

    private ProfilesScreenModel? _model;
    private string _profilesAbsolutePath = string.Empty;

    private ItemList? _list;
    private Label? _nameLabel;
    private Label? _deathsLabel;
    private Label? _placeholderLabel;
    private Label? _errorLabel;
    private LineEdit? _nameEdit;
    private Button? _createButton;
    private Button? _renameButton;
    private Button? _deleteButton;
    private Button? _confirmButton;
    private Button? _cancelButton;
    private Button? _backButton;
    private Control? _detailContent;

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
        _confirmButton = GetNode<Button>("Margin/HBox/Right/VBox/EditButtonRow/ConfirmButton");
        _cancelButton = GetNode<Button>("Margin/HBox/Right/VBox/EditButtonRow/CancelButton");
        _backButton = GetNode<Button>("Margin/HBox/Left/VBox/BackButton");
        _detailContent = GetNode<Control>("Margin/HBox/Right/VBox/DetailContent");

        _profilesAbsolutePath = ProjectSettings.GlobalizePath(WorldHostHooks.DefaultPlayerProfilesResPath);
        var catalog = WorldHostHooks.RequirePlayerProfiles(_profilesAbsolutePath);
        _model = new ProfilesScreenModel(catalog);

        _list.ItemSelected += OnListItemSelected;
        _createButton.Pressed += OnCreatePressed;
        _renameButton.Pressed += OnRenamePressed;
        _deleteButton.Pressed += OnDeletePressed;
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
        _model.Select(_model.Catalog.Profiles[(int)index].Id);
        RefreshUi();
    }

    private void OnCreatePressed()
    {
        _model?.BeginCreate();
        RefreshUi();
        _nameEdit?.GrabFocus();
    }

    private void OnRenamePressed()
    {
        _model?.BeginRename();
        RefreshUi();
        _nameEdit?.GrabFocus();
    }

    private void OnDeletePressed()
    {
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

        if (_model.ConfirmEdit())
            Persist();
        RefreshUi();
    }

    private void OnCancelPressed()
    {
        _model?.CancelEdit();
        RefreshUi();
    }

    private void OnBackPressed() => ChangeSceneOrThrow(MainMenuScenePath);

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
            || _confirmButton is null
            || _cancelButton is null
            || _detailContent is null)
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
        }

        _nameEdit.Visible = _model.EditMode is ProfilesScreenEditMode.Creating or ProfilesScreenEditMode.Renaming;
        if (_nameEdit.Visible)
            _nameEdit.Text = _model.EditBuffer;

        _confirmButton.Visible = editing;
        _cancelButton.Visible = editing;
        _confirmButton.Text = _model.EditMode == ProfilesScreenEditMode.ConfirmDelete ? "Delete" : "Confirm";

        _errorLabel.Visible = !string.IsNullOrEmpty(_model.Error);
        _errorLabel.Text = _model.Error ?? string.Empty;

        _createButton.Disabled = editing;
        _renameButton.Disabled = editing || selected is null;
        _deleteButton.Disabled = editing || selected is null;
        _list.MouseFilter = editing ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;

        if (_model.EditMode == ProfilesScreenEditMode.ConfirmDelete && selected is not null)
        {
            _nameLabel.Text = $"Delete {selected.Name}?";
            _deathsLabel.Text = "This cannot be undone.";
        }
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
