using Minimap.Client.Lobby;
using Minimap.Client.LocalPlay;
using Minimap.Client.Profiles;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class LobbyProfileSelectionTests
{
    private static readonly InputDeviceId Pad0 = InputDeviceId.Joypad(0);
    private static readonly InputDeviceId Pad1 = InputDeviceId.Joypad(1);

    private static LobbyStateMachine CreateLobbyWithProfiles(params string[] names)
    {
        var catalog = new PlayerProfileCatalog();
        foreach (var name in names)
            Assert.True(catalog.TryCreate(name, out _, out _));
        var lobby = new LobbyStateMachine();
        lobby.ConfigureProfiles(catalog);
        return lobby;
    }

    [Fact]
    public void Claim_enters_selecting_profile_and_confirm_advances()
    {
        var lobby = CreateLobbyWithProfiles("Alex");
        Assert.True(lobby.TryClaim(Pad0, out var slot));
        Assert.Equal(LobbySlotMode.SelectingProfile, lobby.GetMode(slot));
        Assert.False(lobby.TryReady(Pad0));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.Equal(LobbySlotMode.SelectingAccessories, lobby.GetMode(slot));
        Assert.True(lobby.TryReady(Pad0));
        Assert.Equal(LobbySlotMode.Ready, lobby.GetMode(slot));
    }

    [Fact]
    public void Cannot_confirm_without_available_profiles()
    {
        var lobby = new LobbyStateMachine();
        lobby.ConfigureProfiles(new PlayerProfileCatalog());
        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.False(lobby.TryConfirmProfile(Pad0));
        Assert.Equal(LobbySlotMode.SelectingProfile, lobby.GetMode(0));
    }

    [Fact]
    public void Carousel_excludes_profiles_confirmed_by_other_slots()
    {
        var lobby = CreateLobbyWithProfiles("Alex", "Blake");
        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.True(lobby.TryClaim(Pad1, out _));

        var available = lobby.GetAvailableProfilesForSlot(1);
        Assert.Single(available);
        Assert.Equal("Blake", available[0].Name);
    }

    [Fact]
    public void Back_from_accessories_keeps_profile_and_accessory_picks()
    {
        var gun = new AccessoryDefinition("gun", Array.Empty<AccessoryEffect>(), pointCost: 1, displayName: "Gun");
        var lobby = CreateLobbyWithProfiles("Alex");
        lobby.ConfigureAccessories(2, [gun]);
        Assert.True(lobby.TryClaim(Pad0, out var slot));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.True(lobby.GetAccessorySelection(slot)!.TryTake(gun));
        Assert.True(lobby.TryBack(Pad0));
        Assert.Equal(LobbySlotMode.SelectingProfile, lobby.GetMode(slot));
        Assert.Equal(["gun"], lobby.GetAccessorySelection(slot)!.Owned.Select(a => a.Id));
        Assert.NotNull(lobby.GetProfileSelection(slot)!.ConfirmedProfileId);
    }

    [Fact]
    public void BuildRoster_includes_profile_id_and_name()
    {
        var lobby = CreateLobbyWithProfiles("Alex");
        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.True(lobby.TryReady(Pad0));

        var roster = lobby.BuildRoster();
        Assert.Equal("Alex", roster.Players[0].DisplayName);
        Assert.NotNull(roster.Players[0].ProfileId);
        Assert.Null(roster.Players[0].AvatarFile);
    }

    [Fact]
    public void BuildRoster_includes_avatar_file_when_set()
    {
        var catalog = new PlayerProfileCatalog();
        Assert.True(catalog.TryCreate("Alex", out var alex, out _));
        var avatarFile = $"{alex!.Id:D}.png";
        Assert.True(catalog.TrySetAvatar(alex.Id, avatarFile, out _));
        var lobby = new LobbyStateMachine();
        lobby.ConfigureProfiles(catalog);

        Assert.True(lobby.TryClaim(Pad0, out _));
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.True(lobby.TryReady(Pad0));

        var roster = lobby.BuildRoster();
        Assert.Equal(avatarFile, roster.Players[0].AvatarFile);
    }

    [Fact]
    public void Cycle_profile_moves_carousel()
    {
        var lobby = CreateLobbyWithProfiles("Alex", "Blake");
        Assert.True(lobby.TryClaim(Pad0, out var slot));
        Assert.True(lobby.TryCycleProfile(Pad0, 1));
        var available = lobby.GetAvailableProfilesForSlot(slot);
        Assert.Equal(1, lobby.GetProfileSelection(slot)!.CarouselIndex);
        Assert.True(lobby.TryConfirmProfile(Pad0));
        Assert.Equal(available[1].Id, lobby.GetProfileSelection(slot)!.ConfirmedProfileId);
    }
}
