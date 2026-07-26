using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

public class AbilityLoadoutTests
{
    [Fact]
    public void Rebuild_separates_dedicated_and_modal()
    {
        var gun = new AccessoryDefinition(
            "gun",
            [new TestShootEffect(1.25f, 200f, 25)],
            activation: new AccessoryActivation(
                AccessoryActivationKind.Dedicated,
                AccessoryActivationBinds.PrimaryFire));
        var plant = new AccessoryDefinition(
            "plant",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));

        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        character.AddAccessory(gun.CreateInstance());
        character.AddAccessory(plant.CreateInstance());

        Assert.True(character.AbilityLoadout.TryGetDedicated(
            AccessoryActivationBinds.PrimaryFire, out var dedicated));
        Assert.Equal("gun", dedicated!.Definition.Id);
        Assert.Single(character.AbilityLoadout.Modal);
        Assert.Equal("plant", character.AbilityLoadout.Modal[0].Definition.Id);
    }

    [Fact]
    public void Rebuild_maps_secondary_fire_dedicated_bind()
    {
        var swing = new AccessoryDefinition(
            "swing",
            [new TestSwingEffect(0.8f, 30, 26f)],
            activation: new AccessoryActivation(
                AccessoryActivationKind.Dedicated,
                AccessoryActivationBinds.SecondaryFire));
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        character.AddAccessory(swing.CreateInstance());

        Assert.True(character.AbilityLoadout.TryGetDedicated(
            AccessoryActivationBinds.SecondaryFire, out var dedicated));
        Assert.Equal("swing", dedicated!.Definition.Id);
    }

    [Fact]
    public void SelectModal_clamps_to_available_slots()
    {
        var plant = new AccessoryDefinition(
            "plant",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        character.AddAccessory(plant.CreateInstance());

        character.AbilityLoadout.SelectModal(3);
        Assert.Equal(0, character.AbilityLoadout.SelectedModalIndex);
        Assert.Equal("plant", character.AbilityLoadout.SelectedModal!.Definition.Id);
    }

    [Fact]
    public void Rebuild_includes_more_than_four_modal_accessories()
    {
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        for (var i = 0; i < 5; i++)
        {
            var def = new AccessoryDefinition(
                $"modal_{i}",
                Array.Empty<AccessoryEffect>(),
                activation: new AccessoryActivation(AccessoryActivationKind.Modal));
            character.AddAccessory(def.CreateInstance());
        }

        Assert.Equal(5, character.AbilityLoadout.Modal.Count);
        Assert.Equal("modal_0", character.AbilityLoadout.SelectedModal!.Definition.Id);
    }

    [Fact]
    public void CycleModal_wraps_through_none_slot()
    {
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        foreach (var id in new[] { "a", "b", "c" })
        {
            var def = new AccessoryDefinition(
                id,
                Array.Empty<AccessoryEffect>(),
                activation: new AccessoryActivation(AccessoryActivationKind.Modal));
            character.AddAccessory(def.CreateInstance());
        }

        Assert.Equal(0, character.AbilityLoadout.SelectedModalIndex);

        character.AbilityLoadout.CycleModal(1);
        Assert.Equal(1, character.AbilityLoadout.SelectedModalIndex);
        character.AbilityLoadout.CycleModal(1);
        Assert.Equal(2, character.AbilityLoadout.SelectedModalIndex);
        character.AbilityLoadout.CycleModal(1);
        Assert.Equal(-1, character.AbilityLoadout.SelectedModalIndex);
        Assert.Null(character.AbilityLoadout.SelectedModal);

        character.AbilityLoadout.CycleModal(1);
        Assert.Equal(0, character.AbilityLoadout.SelectedModalIndex);

        character.AbilityLoadout.SelectModal(-1);
        character.AbilityLoadout.CycleModal(-1);
        Assert.Equal(2, character.AbilityLoadout.SelectedModalIndex);
    }

    [Fact]
    public void Rebuild_preserves_unequipped_selection()
    {
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        character.AddAccessory(new AccessoryDefinition(
            "a",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        character.AddAccessory(new AccessoryDefinition(
            "b",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());

        character.AbilityLoadout.SelectModal(-1);
        Assert.Null(character.AbilityLoadout.SelectedModal);

        character.AddAccessory(new AccessoryDefinition(
            "c",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());

        Assert.Equal(-1, character.AbilityLoadout.SelectedModalIndex);
        Assert.Null(character.AbilityLoadout.SelectedModal);
        Assert.Equal(3, character.AbilityLoadout.Modal.Count);
    }

    [Fact]
    public void CycleModal_no_ops_when_pool_empty()
    {
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        character.AbilityLoadout.CycleModal(1);
        character.AbilityLoadout.CycleModal(-1);
        Assert.Empty(character.AbilityLoadout.Modal);
        Assert.Null(character.AbilityLoadout.SelectedModal);
    }
}
