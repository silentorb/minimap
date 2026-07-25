using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Client.Profiles;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class LobbyStateMachineTests
{
    private static readonly InputDeviceId Keyboard = InputDeviceId.Keyboard;
    private static readonly InputDeviceId Pad0 = InputDeviceId.Joypad(0);
    private static readonly InputDeviceId Pad1 = InputDeviceId.Joypad(1);

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
    public void Starts_with_four_available_slots()
    {
        var lobby = new LobbyStateMachine();
        for (var i = 0; i < LobbyStateMachine.SlotCount; i++)
            Assert.Equal(LobbySlotMode.Available, lobby.GetMode(i));
        Assert.Equal(0, lobby.ClaimedCount);
        Assert.False(lobby.CanStartGame);
    }

    [Fact]
    public void Claim_assigns_lowest_available_slot()
    {
        var lobby = CreateLobby("Alex");
        Assert.True(lobby.TryClaim(Pad1, out var slot));
        Assert.Equal(0, slot);
        Assert.Equal(LobbySlotMode.SelectingProfile, lobby.GetMode(0));
        Assert.True(lobby.IsDeviceBound(Pad1));
    }

    [Fact]
    public void Rejects_double_claim_from_same_device()
    {
        var lobby = CreateLobby("Alex");
        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.False(lobby.TryClaim(Pad0, out _));
    }

    [Fact]
    public void Ready_and_start_gate_requires_all_claimed_ready()
    {
        var lobby = CreateLobby("Alex", "Blake");
        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.False(lobby.CanStartGame);
        Assert.True(lobby.TryReady(Pad0));
        Assert.True(lobby.CanStartGame);

        Assert.True(lobby.TryClaim(Pad1, out _));
        Assert.True(lobby.TryConfirmProfile(Pad1));
        Assert.False(lobby.CanStartGame);
        Assert.True(lobby.TryReady(Pad1));
        Assert.True(lobby.CanStartGame);
    }

    [Fact]
    public void Back_steps_ready_accessories_profile_available_and_releases_devices()
    {
        var lobby = CreateLobby("Alex");
        Assert.True(lobby.TryClaim(Pad0, out var slot));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.True(lobby.TryReady(Pad0));
        Assert.True(lobby.TryBack(Pad0));
        Assert.Equal(LobbySlotMode.SelectingAccessories, lobby.GetMode(slot));
        Assert.True(lobby.TryBack(Pad0));
        Assert.Equal(LobbySlotMode.SelectingProfile, lobby.GetMode(slot));
        Assert.True(lobby.TryBack(Pad0));
        Assert.Equal(LobbySlotMode.Available, lobby.GetMode(slot));
        Assert.False(lobby.IsDeviceBound(Pad0));
    }

    [Fact]
    public void BuildRoster_includes_claimed_devices_only()
    {
        var lobby = CreateLobby("Alex", "Blake");
        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.True(lobby.TryReady(Pad0));
        Assert.True(lobby.TryClaim(Pad1, out _));

        var roster = lobby.BuildRoster();
        Assert.Equal(2, roster.PlayerCount);
        Assert.True(roster.Players[0].HasJoypad(0));
        Assert.True(roster.Players[1].HasJoypad(1));
    }
}

public class LocalPlayRosterTests
{
    [Fact]
    public void Default_solo_keyboard_has_one_player()
    {
        var roster = new LocalPlayRoster();
        roster.ApplyDefaultSoloKeyboard();
        Assert.Equal(1, roster.PlayerCount);
        Assert.True(roster.Players[0].HasKeyboard);
        Assert.Null(roster.Players[0].ProfileId);
    }

    [Fact]
    public void Player_supports_multiple_devices()
    {
        var roster = new LocalPlayRoster();
        roster.SetPlayerCount(1);
        roster.Players[0].AddDevice(InputDeviceId.Keyboard);
        roster.Players[0].AddDevice(InputDeviceId.Joypad(0));
        Assert.Equal(2, roster.Players[0].Devices.Count);
    }

    [Fact]
    public void CopyFrom_preserves_devices_profile_and_selected_accessories()
    {
        var farm = new AccessoryDefinition(
            "farm",
            Array.Empty<AccessoryEffect>(),
            pointCost: 1,
            displayName: "Farm");
        var profileId = Guid.NewGuid();
        var source = new LocalPlayRoster();
        source.SetPlayerCount(1);
        source.Players[0].AddDevice(InputDeviceId.Joypad(0));
        source.Players[0].SetSelectedAccessories([farm]);
        source.Players[0].SetProfile(profileId, "Alex");

        var target = new LocalPlayRoster();
        target.CopyFrom(source);

        Assert.Equal(1, target.PlayerCount);
        Assert.True(target.Players[0].HasJoypad(0));
        Assert.Equal(["farm"], target.Players[0].SelectedAccessories.Select(a => a.Id));
        Assert.Equal(profileId, target.Players[0].ProfileId);
        Assert.Equal("Alex", target.Players[0].DisplayName);
    }
}

public class ReconnectStateTests
{
    [Fact]
    public void CanRebind_only_unassigned_joypad()
    {
        var reconnect = new ReconnectState();
        var roster = new LocalPlayRoster();
        roster.SetPlayerCount(2);
        roster.Players[0].AddDevice(InputDeviceId.Joypad(0));
        roster.Players[1].AddDevice(InputDeviceId.Joypad(1));
        reconnect.BeginWait(0);

        Assert.True(reconnect.CanRebindJoypad(roster, 0));
        Assert.False(reconnect.CanRebindJoypad(roster, 1));
    }

    [Fact]
    public void CanDrop_when_other_player_has_connected_device()
    {
        var reconnect = new ReconnectState();
        var roster = new LocalPlayRoster();
        roster.SetPlayerCount(2);
        roster.Players[0].AddDevice(InputDeviceId.Joypad(0));
        roster.Players[1].AddDevice(InputDeviceId.Keyboard);
        reconnect.BeginWait(0);

        Assert.True(reconnect.CanDropPlayer(
            roster,
            InputDeviceId.Keyboard,
            _ => false));
    }
}
