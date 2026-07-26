using Minimap.App;
using Minimap.Client;
using Minimap.Client.LocalPlay;
using Minimap.Client.Profiles;
using Xunit;

namespace Minimap.App.Tests;

public class PlayerAvatarStoreTests
{
    [Fact]
    public void TryImport_copies_file_and_replaces_previous()
    {
        var root = Path.Combine(Path.GetTempPath(), "minimap-avatars-" + Guid.NewGuid().ToString("N"));
        var avatarsDir = Path.Combine(root, "profile_avatars");
        var sourceA = Path.Combine(root, "a.png");
        var sourceB = Path.Combine(root, "b.jpg");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(sourceA, [1, 2, 3]);
        File.WriteAllBytes(sourceB, [4, 5, 6]);
        var profileId = Guid.NewGuid();

        try
        {
            Assert.True(PlayerAvatarStore.TryImport(
                avatarsDir,
                profileId,
                sourceA,
                previousAvatarFile: null,
                out var firstFile,
                out var firstError));
            Assert.Null(firstError);
            Assert.Equal($"{profileId:D}.png", firstFile);
            Assert.True(File.Exists(Path.Combine(avatarsDir, firstFile!)));

            Assert.True(PlayerAvatarStore.TryImport(
                avatarsDir,
                profileId,
                sourceB,
                previousAvatarFile: firstFile,
                out var secondFile,
                out var secondError));
            Assert.Null(secondError);
            Assert.Equal($"{profileId:D}.jpg", secondFile);
            Assert.True(File.Exists(Path.Combine(avatarsDir, secondFile!)));
            Assert.False(File.Exists(Path.Combine(avatarsDir, firstFile!)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TryImport_rejects_unsupported_extension_and_oversized_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "minimap-avatars-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var badExt = Path.Combine(root, "x.gif");
        var oversized = Path.Combine(root, "big.png");
        File.WriteAllText(badExt, "nope");
        File.WriteAllBytes(oversized, new byte[PlayerAvatarStore.MaxSourceBytes + 1]);

        try
        {
            Assert.False(PlayerAvatarStore.TryImport(
                Path.Combine(root, "avatars"),
                Guid.NewGuid(),
                badExt,
                null,
                out _,
                out var extError));
            Assert.Contains("PNG", extError, StringComparison.OrdinalIgnoreCase);

            Assert.False(PlayerAvatarStore.TryImport(
                Path.Combine(root, "avatars"),
                Guid.NewGuid(),
                oversized,
                null,
                out _,
                out var sizeError));
            Assert.Contains("8 MiB", sizeError, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TryDelete_removes_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "minimap-avatars-" + Guid.NewGuid().ToString("N"));
        var avatarsDir = Path.Combine(root, "profile_avatars");
        Directory.CreateDirectory(avatarsDir);
        var fileName = $"{Guid.NewGuid():D}.png";
        var path = Path.Combine(avatarsDir, fileName);
        File.WriteAllBytes(path, [9]);

        try
        {
            Assert.True(PlayerAvatarStore.TryDelete(avatarsDir, fileName, out var error));
            Assert.Null(error);
            Assert.False(File.Exists(path));
            Assert.True(PlayerAvatarStore.TryDelete(avatarsDir, null, out _));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Store_round_trips_avatarFile_and_rejects_absolute_paths()
    {
        var dir = Path.Combine(Path.GetTempPath(), "minimap-profiles-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "player_profiles.json");

        try
        {
            var catalog = new PlayerProfileCatalog();
            Assert.True(catalog.TryCreate("Alex", out var alex, out _));
            var avatarFile = $"{alex!.Id:D}.png";
            Assert.True(catalog.TrySetAvatar(alex.Id, avatarFile, out _));
            PlayerProfileStore.SaveToFile(path, catalog);

            var loaded = PlayerProfileStore.LoadFromFile(path);
            Assert.Equal(avatarFile, loaded.Profiles[0].AvatarFile);

            var invalid = """
                {
                  "profiles": [
                    {
                      "id": "11111111-1111-1111-1111-111111111111",
                      "name": "Bad",
                      "deaths": 0,
                      "avatarFile": "/tmp/evil.png"
                    }
                  ]
                }
                """;
            File.WriteAllText(path, invalid);
            Assert.Throws<InvalidOperationException>(() => PlayerProfileStore.LoadFromFile(path));
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Catalog_set_and_clear_avatar()
    {
        var catalog = new PlayerProfileCatalog();
        Assert.True(catalog.TryCreate("Alex", out var alex, out _));
        Assert.True(catalog.TrySetAvatar(alex!.Id, $"{alex.Id:D}.webp", out _));
        Assert.Equal($"{alex.Id:D}.webp", alex.AvatarFile);
        Assert.False(catalog.TrySetAvatar(alex.Id, "../escape.png", out var error));
        Assert.Contains("filename", error, StringComparison.OrdinalIgnoreCase);
        Assert.True(catalog.TryClearAvatar(alex.Id));
        Assert.Null(alex.AvatarFile);
    }

    [Fact]
    public void Hook_import_and_delete_are_registered()
    {
        var root = Path.Combine(Path.GetTempPath(), "minimap-avatar-hooks-" + Guid.NewGuid().ToString("N"));
        var avatarsDir = Path.Combine(root, "profile_avatars");
        var source = Path.Combine(root, "face.png");
        Directory.CreateDirectory(root);
        File.WriteAllBytes(source, [1]);
        var profileId = Guid.NewGuid();

        try
        {
            var import = WorldHostHooks.RequireImportPlayerAvatar(avatarsDir, profileId, source, null);
            Assert.True(import.Ok);
            Assert.Equal($"{profileId:D}.png", import.AvatarFile);

            var delete = WorldHostHooks.RequireDeletePlayerAvatar(avatarsDir, import.AvatarFile);
            Assert.True(delete.Ok);
            Assert.False(File.Exists(Path.Combine(avatarsDir, import.AvatarFile!)));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}

public class ProfileAvatarRosterTests
{
    [Fact]
    public void LocalPlayerEntry_and_CopyFrom_carry_avatar_file()
    {
        var profileId = Guid.NewGuid();
        var source = new LocalPlayRoster();
        source.SetPlayerCount(1);
        source.Players[0].SetProfile(profileId, "Alex", $"{profileId:D}.png");

        var target = new LocalPlayRoster();
        target.CopyFrom(source);

        Assert.Equal($"{profileId:D}.png", target.Players[0].AvatarFile);

        target.Players[0].ClearProfile();
        Assert.Null(target.Players[0].AvatarFile);
    }

    [Fact]
    public void PlayerHudModel_includes_optional_avatar_path()
    {
        var model = new PlayerHudModel
        {
            DisplayName = "Alex",
            AvatarAbsolutePath = "/tmp/avatar.png",
            Resources = Array.Empty<PlayerHudResourceModel>(),
        };
        Assert.Equal("/tmp/avatar.png", model.AvatarAbsolutePath);
    }
}
