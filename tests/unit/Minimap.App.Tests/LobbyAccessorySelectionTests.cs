using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Client.Profiles;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class LobbyAccessorySelectionTests
{
    private static AccessoryDefinition MakeAccessory(string id, int cost) =>
        new(id, Array.Empty<AccessoryEffect>(), pointCost: cost, displayName: id);

    private static LobbyStateMachine CreateLobby(params string[] profileNames)
    {
        var catalog = new PlayerProfileCatalog();
        foreach (var name in profileNames)
            Assert.True(catalog.TryCreate(name, out _, out _));
        var lobby = new LobbyStateMachine();
        lobby.ConfigureProfiles(catalog);
        return lobby;
    }

    [Fact]
    public void Choices_persist_accessories_to_ready_and_back()
    {
        var gun = MakeAccessory("gun", 1);
        var plant = MakeAccessory("plant", 1);
        var lobby = CreateLobby("Alex");
        lobby.ConfigureAccessories(2, [gun, plant]);

        Assert.True(lobby.TryClaim(InputDeviceId.Keyboard, out var slot));
        Assert.True(lobby.TryConfirmProfile(InputDeviceId.Keyboard));
        var selection = lobby.GetAccessorySelection(slot)!;
        Assert.True(selection.TryTake(gun));
        Assert.Equal(1, selection.RemainingPoints);

        Assert.True(lobby.TryReady(InputDeviceId.Keyboard));
        Assert.Same(selection, lobby.GetAccessorySelection(slot));
        Assert.Equal(["gun"], selection.Owned.Select(a => a.Id));

        Assert.True(lobby.TryBack(InputDeviceId.Keyboard));
        Assert.Equal(LobbySlotMode.SelectingAccessories, lobby.GetMode(slot));
        Assert.Same(selection, lobby.GetAccessorySelection(slot));
        Assert.Equal(["gun"], selection.Owned.Select(a => a.Id));
    }

    [Fact]
    public void Back_to_available_clears_selection()
    {
        var gun = MakeAccessory("gun", 1);
        var lobby = CreateLobby("Alex");
        lobby.ConfigureAccessories(2, [gun]);
        Assert.True(lobby.TryClaim(InputDeviceId.Keyboard, out var slot));
        Assert.True(lobby.GetAccessorySelection(slot)!.TryTake(gun));
        Assert.True(lobby.TryBack(InputDeviceId.Keyboard));
        Assert.Equal(LobbySlotMode.Available, lobby.GetMode(slot));
        Assert.Null(lobby.GetAccessorySelection(slot));
    }

    [Fact]
    public void BuildRoster_copies_selected_accessories()
    {
        var gun = MakeAccessory("gun", 1);
        var lobby = CreateLobby("Alex");
        lobby.ConfigureAccessories(2, [gun]);
        Assert.True(lobby.TryClaim(InputDeviceId.Keyboard, out _));
        Assert.True(lobby.TryConfirmProfile(InputDeviceId.Keyboard));
        Assert.True(lobby.GetAccessorySelection(0)!.TryTake(gun));
        Assert.True(lobby.TryReady(InputDeviceId.Keyboard));

        var roster = lobby.BuildRoster();
        Assert.Equal(["gun"], roster.Players[0].SelectedAccessories.Select(a => a.Id));
    }

    [Fact]
    public void Lobby_to_world_roster_keeps_selected_accessories()
    {
        var farm = MakeAccessory("farm", 1);
        var geek = MakeAccessory("geek", 1);
        var lobby = CreateLobby("Alex");
        lobby.ConfigureAccessories(2, [farm, geek]);
        Assert.True(lobby.TryClaim(InputDeviceId.Joypad(0), out _));
        Assert.True(lobby.TryConfirmProfile(InputDeviceId.Joypad(0)));
        Assert.True(lobby.GetAccessorySelection(0)!.TryTake(farm));
        Assert.True(lobby.GetAccessorySelection(0)!.TryTake(geek));
        Assert.True(lobby.TryReady(InputDeviceId.Joypad(0)));

        var worldRoster = new LocalPlayRoster();
        worldRoster.CopyFrom(lobby.BuildRoster());

        Assert.Equal(["farm", "geek"], worldRoster.Players[0].SelectedAccessories.Select(a => a.Id));
        Assert.True(worldRoster.Players[0].HasJoypad(0));
        Assert.Equal("Alex", worldRoster.Players[0].DisplayName);
    }

    [Fact]
    public void Prior_owned_cannot_be_returned()
    {
        var gun = MakeAccessory("gun", 1);
        var state = new LobbyAccessorySelectionState(2, priorOwned: [gun]);
        Assert.False(state.TryReturn(gun));
        Assert.True(state.IsLocked(gun));
    }

    [Fact]
    public void Take_respects_point_budget()
    {
        var gun = MakeAccessory("gun", 2);
        var plant = MakeAccessory("plant", 1);
        var state = new LobbyAccessorySelectionState(2);
        Assert.True(state.TryTake(gun));
        Assert.False(state.TryTake(plant));
        Assert.Equal(0, state.RemainingPoints);
    }

    [Fact]
    public void Return_refunds_points()
    {
        var gun = MakeAccessory("gun", 1);
        var state = new LobbyAccessorySelectionState(2);
        Assert.True(state.TryTake(gun));
        Assert.True(state.TryReturn(gun));
        Assert.Equal(2, state.RemainingPoints);
    }

    [Fact]
    public void Prior_owned_still_counts_against_available_catalog()
    {
        var gun = MakeAccessory("gun", 1);
        var state = new LobbyAccessorySelectionState(2, priorOwned: [gun]);
        Assert.Contains(gun, state.Owned);
        Assert.False(state.TryTake(gun));
    }
}
