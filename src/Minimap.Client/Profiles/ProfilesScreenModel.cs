namespace Minimap.Client.Profiles;

public enum ProfilesScreenEditMode
{
    None,
    Creating,
    Renaming,
    ConfirmDelete,
}

/// <summary>Pure Profiles screen state: list selection and create/rename/delete.</summary>
public sealed class ProfilesScreenModel
{
    private readonly PlayerProfileCatalog _catalog;
    private Guid? _selectedId;
    private ProfilesScreenEditMode _editMode = ProfilesScreenEditMode.None;
    private string _editBuffer = string.Empty;
    private string? _error;

    public ProfilesScreenModel(PlayerProfileCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
        if (_catalog.Profiles.Count > 0)
            _selectedId = _catalog.Profiles[0].Id;
    }

    public PlayerProfileCatalog Catalog => _catalog;

    public Guid? SelectedId => _selectedId;

    public PlayerProfileRecord? SelectedProfile =>
        _selectedId is Guid id ? _catalog.Find(id) : null;

    public ProfilesScreenEditMode EditMode => _editMode;

    public string EditBuffer => _editBuffer;

    public string? Error => _error;

    public void Select(Guid id)
    {
        if (_catalog.Find(id) is null)
            return;
        CancelEdit();
        _selectedId = id;
    }

    public void BeginCreate()
    {
        _editMode = ProfilesScreenEditMode.Creating;
        _editBuffer = string.Empty;
        _error = null;
    }

    public void BeginRename()
    {
        var selected = SelectedProfile;
        if (selected is null)
            return;
        _editMode = ProfilesScreenEditMode.Renaming;
        _editBuffer = selected.Name;
        _error = null;
    }

    public void BeginDelete()
    {
        if (SelectedProfile is null)
            return;
        _editMode = ProfilesScreenEditMode.ConfirmDelete;
        _error = null;
    }

    public void SetEditBuffer(string value)
    {
        _editBuffer = value ?? string.Empty;
        _error = null;
    }

    public void CancelEdit()
    {
        _editMode = ProfilesScreenEditMode.None;
        _editBuffer = string.Empty;
        _error = null;
    }

    public bool ConfirmEdit()
    {
        switch (_editMode)
        {
            case ProfilesScreenEditMode.Creating:
                if (!_catalog.TryCreate(_editBuffer, out var created, out var createError))
                {
                    _error = createError;
                    return false;
                }

                _selectedId = created!.Id;
                CancelEdit();
                return true;

            case ProfilesScreenEditMode.Renaming:
                if (_selectedId is not Guid renameId)
                {
                    _error = "No profile selected.";
                    return false;
                }

                if (!_catalog.TryRename(renameId, _editBuffer, out var renameError))
                {
                    _error = renameError;
                    return false;
                }

                CancelEdit();
                return true;

            case ProfilesScreenEditMode.ConfirmDelete:
                if (_selectedId is not Guid deleteId)
                    return false;
                if (!_catalog.TryDelete(deleteId))
                    return false;
                _selectedId = _catalog.Profiles.Count > 0 ? _catalog.Profiles[0].Id : null;
                CancelEdit();
                return true;

            default:
                return false;
        }
    }
}
