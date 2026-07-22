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
}
