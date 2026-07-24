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
            [new TestShootEffect(1.25f, 200f, 25f)],
            activation: new AccessoryActivation(
                AccessoryActivationKind.Dedicated,
                AccessoryActivationBinds.PrimaryFire));
        var plant = new AccessoryDefinition(
            "plant",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));

        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare);
        character.AddAccessory(gun.CreateInstance());
        character.AddAccessory(plant.CreateInstance());

        Assert.True(character.AbilityLoadout.TryGetDedicated(
            AccessoryActivationBinds.PrimaryFire, out var dedicated));
        Assert.Equal("gun", dedicated!.Definition.Id);
        Assert.Single(character.AbilityLoadout.Modal);
        Assert.Equal("plant", character.AbilityLoadout.Modal[0].Definition.Id);
    }

    [Fact]
    public void SelectModal_clamps_to_available_slots()
    {
        var plant = new AccessoryDefinition(
            "plant",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));
        var character = new Character(0, 1, SimVec2.Zero, TestContent.Bare);
        character.AddAccessory(plant.CreateInstance());

        character.AbilityLoadout.SelectModal(3);
        Assert.Equal(0, character.AbilityLoadout.SelectedModalIndex);
        Assert.Equal("plant", character.AbilityLoadout.SelectedModal!.Definition.Id);
    }
}
