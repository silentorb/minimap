using Minimap.App;
using Minimap.Client.Achievements;
using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Client.PostSession;
using Minimap.Client.Profiles;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class AchievementPersistenceTests
{
    [Fact]
    public void Unlock_and_store_round_trip()
    {
        var dir = Path.Combine(Path.GetTempPath(), "minimap-achievements-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "player_profiles.json");
        try
        {
            var catalog = new PlayerProfileCatalog();
            Assert.True(catalog.TryCreate("Alex", out var alex, out _));
            Assert.True(catalog.TryUnlockAchievement(alex!.Id, AchievementIds.Survive5Minutes));
            Assert.False(catalog.TryUnlockAchievement(alex.Id, AchievementIds.Survive5Minutes));
            PlayerProfileStore.SaveToFile(path, catalog);

            var loaded = PlayerProfileStore.LoadFromFile(path);
            Assert.True(loaded.Profiles[0].HasAchievement(AchievementIds.Survive5Minutes));
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}

public class SessionAchievementLedgerTests
{
    [Fact]
    public void Records_first_earn_once_per_session()
    {
        var ledger = new SessionAchievementLedger();
        ledger.EnsurePlayerCount(1);
        Assert.True(ledger.TryRecord(0, AchievementIds.Survive5Minutes, firstTime: true));
        Assert.False(ledger.TryRecord(0, AchievementIds.Survive5Minutes, firstTime: false));
        Assert.Single(ledger.GetEarns(0));
        Assert.True(ledger.GetEarns(0)[0].FirstTime);
    }
}

public class PostSessionReadyModelTests
{
    [Fact]
    public void All_ready_requires_every_player()
    {
        var model = new PostSessionReadyModel(2);
        Assert.False(model.AllReady);
        Assert.True(model.TrySetReady(0, true));
        Assert.False(model.AllReady);
        Assert.True(model.TrySetReady(1, true));
        Assert.True(model.AllReady);
    }
}

public class LobbyRestoreFromRosterTests
{
    [Fact]
    public void ApplyFromRoster_lands_in_selecting_accessories_and_skips_disconnected_joypad()
    {
        var catalog = new PlayerProfileCatalog();
        Assert.True(catalog.TryCreate("Alex", out var alex, out _));
        Assert.True(catalog.TryCreate("Blake", out var blake, out _));

        var lobby = new LobbyStateMachine();
        lobby.ConfigureProfiles(catalog);
        lobby.ConfigureAccessories(2, Array.Empty<AccessoryDefinition>());

        var roster = new LocalPlayRoster();
        roster.SetPlayerCount(2);
        roster.Players[0].AddDevice(InputDeviceId.Keyboard);
        roster.Players[0].SetProfile(alex!.Id, alex.Name);
        roster.Players[1].AddDevice(InputDeviceId.Joypad(0));
        roster.Players[1].SetProfile(blake!.Id, blake.Name);

        var restored = lobby.ApplyFromRoster(
            roster,
            device => !device.IsJoypad || device.JoypadDevice != 0);

        Assert.Equal(1, restored);
        Assert.Equal(LobbySlotMode.SelectingAccessories, lobby.GetMode(0));
        Assert.True(lobby.IsDeviceBound(InputDeviceId.Keyboard));
        Assert.False(lobby.IsDeviceBound(InputDeviceId.Joypad(0)));
        Assert.Equal(alex.Id, lobby.GetProfileSelection(0)!.ConfirmedProfileId);
        Assert.Equal(LobbySlotMode.Available, lobby.GetMode(1));
    }
}
