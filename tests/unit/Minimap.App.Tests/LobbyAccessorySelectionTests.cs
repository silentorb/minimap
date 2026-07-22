using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class LobbyAccessorySelectionTests
{
    private static AccessoryDefinition MakeAccessory(string id, int cost) =>
        new(id, Array.Empty<AccessoryEffect>(), pointCost: cost, displayName: id);

    [Fact]
    public void Choices_persist_claimed_to_ready_and_back()
    {
        var gun = MakeAccessory("gun", 1);
        var plant = MakeAccessory("plant", 1);
        var lobby = new LobbyStateMachine();
        lobby.ConfigureAccessories(2, [gun, plant]);

        Assert.True(lobby.TryClaim(InputDeviceId.Keyboard, out var slot));
        var selection = lobby.GetAccessorySelection(slot)!;
        Assert.True(selection.TryTake(gun));
        Assert.Equal(1, selection.RemainingPoints);

        Assert.True(lobby.TryReady(InputDeviceId.Keyboard));
        Assert.Same(selection, lobby.GetAccessorySelection(slot));
        Assert.Equal(["gun"], selection.Owned.Select(a => a.Id));

        Assert.True(lobby.TryBack(InputDeviceId.Keyboard));
        Assert.Equal(LobbySlotMode.Claimed, lobby.GetMode(slot));
        Assert.Same(selection, lobby.GetAccessorySelection(slot));
        Assert.Equal(["gun"], selection.Owned.Select(a => a.Id));
    }

    [Fact]
    public void Back_to_available_clears_selection()
    {
        var gun = MakeAccessory("gun", 1);
        var lobby = new LobbyStateMachine();
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
        var lobby = new LobbyStateMachine();
        lobby.ConfigureAccessories(2, [gun]);
        Assert.True(lobby.TryClaim(InputDeviceId.Keyboard, out _));
        Assert.True(lobby.GetAccessorySelection(0)!.TryTake(gun));
        Assert.True(lobby.TryReady(InputDeviceId.Keyboard));

        var roster = lobby.BuildRoster();
        Assert.Equal(["gun"], roster.Players[0].SelectedAccessories.Select(a => a.Id));
    }

    [Fact]
    public void Take_moves_to_owned_and_Return_moves_back_to_available()
    {
        var gun = MakeAccessory("gun", 1);
        var plant = MakeAccessory("plant", 1);
        var state = new LobbyAccessorySelectionState(2);
        Assert.True(state.TryTake(gun));
        Assert.Equal(["gun"], state.Owned.Select(a => a.Id));
        Assert.Equal(["gun"], state.StageOwned.Select(a => a.Id));
        Assert.Equal(1, state.RemainingPoints);

        Assert.True(state.TryReturn(gun));
        Assert.Empty(state.Owned);
        Assert.Empty(state.StageOwned);
        Assert.Equal(2, state.RemainingPoints);
        Assert.False(state.IsOwned(gun));
        Assert.False(state.IsOwned(plant));
    }

    [Fact]
    public void Prior_owned_appear_in_owned_but_cannot_be_returned()
    {
        var gun = MakeAccessory("gun", 1);
        var plant = MakeAccessory("plant", 1);
        var state = new LobbyAccessorySelectionState(2, priorOwned: [gun]);

        Assert.Equal(["gun"], state.Owned.Select(a => a.Id));
        Assert.True(state.IsLocked(gun));
        Assert.False(state.TryReturn(gun));
        Assert.Equal(2, state.RemainingPoints);
        Assert.Equal(["gun"], state.Owned.Select(a => a.Id));

        Assert.True(state.TryTake(plant));
        Assert.Equal(["gun", "plant"], state.Owned.Select(a => a.Id));
        Assert.True(state.TryReturn(plant));
        Assert.Equal(["gun"], state.Owned.Select(a => a.Id));
        Assert.Equal(2, state.RemainingPoints);
    }

    [Fact]
    public void Cannot_take_already_owned_including_prior()
    {
        var gun = MakeAccessory("gun", 1);
        var state = new LobbyAccessorySelectionState(2, priorOwned: [gun]);
        Assert.False(state.TryTake(gun));
    }

    [Fact]
    public void ResetStage_clears_stage_choices_but_keeps_prior()
    {
        var gun = MakeAccessory("gun", 1);
        var plant = MakeAccessory("plant", 1);
        var state = new LobbyAccessorySelectionState(2, priorOwned: [gun]);
        Assert.True(state.TryTake(plant));
        state.ResetStage();
        Assert.Equal(["gun"], state.Owned.Select(a => a.Id));
        Assert.Empty(state.StageOwned);
        Assert.Equal(2, state.RemainingPoints);
    }
}
