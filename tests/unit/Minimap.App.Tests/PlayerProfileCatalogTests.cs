using Minimap.App;
using Minimap.Client.Profiles;
using Xunit;

namespace Minimap.App.Tests;

public class PlayerProfileCatalogTests
{
    [Fact]
    public void Create_rename_delete_and_unique_names()
    {
        var catalog = new PlayerProfileCatalog();
        Assert.True(catalog.TryCreate("Alex", out var alex, out _));
        Assert.NotNull(alex);
        Assert.False(catalog.TryCreate("alex", out _, out var dupError));
        Assert.Contains("already exists", dupError, StringComparison.OrdinalIgnoreCase);

        Assert.True(catalog.TryRename(alex!.Id, "Blake", out _));
        Assert.Equal("Blake", catalog.Find(alex.Id)!.Name);

        Assert.True(catalog.TryDelete(alex.Id));
        Assert.Empty(catalog.Profiles);
    }

    [Fact]
    public void Rejects_empty_and_too_long_names()
    {
        var catalog = new PlayerProfileCatalog();
        Assert.False(catalog.TryCreate("   ", out _, out _));
        Assert.False(catalog.TryCreate(new string('a', PlayerProfileCatalog.MaxNameLength + 1), out _, out _));
    }

    [Fact]
    public void Increment_deaths()
    {
        var catalog = new PlayerProfileCatalog();
        Assert.True(catalog.TryCreate("Alex", out var alex, out _));
        Assert.True(catalog.TryIncrementDeaths(alex!.Id));
        Assert.Equal(1, alex.Deaths);
    }

    [Fact]
    public void Store_round_trips_and_missing_file_is_empty()
    {
        var dir = Path.Combine(Path.GetTempPath(), "minimap-profiles-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "player_profiles.json");
        try
        {
            var empty = PlayerProfileStore.LoadFromFile(path);
            Assert.Empty(empty.Profiles);

            Assert.True(empty.TryCreate("Alex", out _, out _));
            PlayerProfileStore.SaveToFile(path, empty);

            var loaded = PlayerProfileStore.LoadFromFile(path);
            Assert.Single(loaded.Profiles);
            Assert.Equal("Alex", loaded.Profiles[0].Name);
            Assert.Equal(0, loaded.Profiles[0].Deaths);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}

public class ProfilesScreenModelTests
{
    [Fact]
    public void Create_select_rename_delete_flow()
    {
        var catalog = new PlayerProfileCatalog();
        var model = new ProfilesScreenModel(catalog);

        model.BeginCreate();
        model.SetEditBuffer("Alex");
        Assert.True(model.ConfirmEdit());
        Assert.Equal("Alex", model.SelectedProfile!.Name);

        model.BeginRename();
        model.SetEditBuffer("Blake");
        Assert.True(model.ConfirmEdit());
        Assert.Equal("Blake", model.SelectedProfile!.Name);

        model.BeginDelete();
        Assert.True(model.ConfirmEdit());
        Assert.Null(model.SelectedProfile);
        Assert.Empty(catalog.Profiles);
    }

    [Fact]
    public void Create_validation_error_stays_in_edit_mode()
    {
        var catalog = new PlayerProfileCatalog();
        catalog.TryCreate("Alex", out _, out _);
        var model = new ProfilesScreenModel(catalog);
        model.BeginCreate();
        model.SetEditBuffer("alex");
        Assert.False(model.ConfirmEdit());
        Assert.Equal(ProfilesScreenEditMode.Creating, model.EditMode);
        Assert.NotNull(model.Error);
    }
}
