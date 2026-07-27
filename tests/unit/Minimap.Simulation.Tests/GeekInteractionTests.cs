using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class GeekInteractionTests
{
    [Fact]
    public void UseComputer_interact_succeeds_on_computer_with_geek_selected()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var computerDef = new ActorDefinition("computer");
        var geek = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        geek.AddAccessory(new AccessoryDefinition(
            "geek",
            [new TestUseComputerEffect(TestContent.EnergyResource.Tag, 1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        geek.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(geek, w.HexSize);
        Assert.True(w.TryPlaceActor(front, computerDef, geek.FactionId));
        Assert.Equal(geek.FactionId, w.CellActors[front].FactionId);

        var energyBefore = geek.Energy;
        Assert.NotNull(EnvironmentInteraction.ResolveTarget(w, geek));
        Assert.True(EnvironmentInteraction.TryInteract(w, geek));
        Assert.Equal(energyBefore - 1, geek.Energy);
        Assert.True(w.IsCellOccupied(front));
    }

    [Fact]
    public void UseComputer_interact_rejects_at_zero_energy()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var computerDef = new ActorDefinition("computer");
        var geek = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        geek.Energy = 0;
        geek.AddAccessory(new AccessoryDefinition(
            "geek",
            [new TestUseComputerEffect(TestContent.EnergyResource.Tag, 1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        geek.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(geek, w.HexSize);
        Assert.True(w.TryPlaceActor(front, computerDef, geek.FactionId));
        Assert.Null(EnvironmentInteraction.ResolveTarget(w, geek));
        Assert.False(EnvironmentInteraction.TryInteract(w, geek));
    }

    [Fact]
    public void UseComputer_interact_rejects_non_computer()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var other = new ActorDefinition("crate");
        var geek = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        geek.AddAccessory(new AccessoryDefinition(
            "geek",
            [new TestUseComputerEffect(TestContent.EnergyResource.Tag, 1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal)).CreateInstance());
        geek.AbilityLoadout.SelectModal(0);

        var front = CellFacing.CellInFront(geek, w.HexSize);
        Assert.True(w.TryPlaceActor(front, other));

        Assert.Null(EnvironmentInteraction.ResolveTarget(w, geek));
        Assert.False(EnvironmentInteraction.TryInteract(w, geek));
        Assert.True(w.IsCellOccupied(front));
    }

    [Fact]
    public void UseComputer_interact_rejects_without_geek_selected()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var computerDef = new ActorDefinition("computer");
        var character = w.AddActor(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);

        var front = CellFacing.CellInFront(character, w.HexSize);
        Assert.True(w.TryPlaceActor(front, computerDef));

        Assert.Null(EnvironmentInteraction.ResolveTarget(w, character));
        Assert.False(EnvironmentInteraction.TryInteract(w, character));
    }

    private sealed class TestUseComputerEffect : AccessoryEffect, IInteractionEffect, IEffectUseCost
    {
        public TestUseComputerEffect(TagId? costResourceTag = null, int costAmount = 0)
        {
            CostResourceTag = costResourceTag;
            CostAmount = costAmount;
        }

        public TagId? CostResourceTag { get; }
        public int CostAmount { get; }

        public bool CanInteract(GameWorld world, Actor actor, Actor target) =>
            EffectUseCosts.CanAfford(actor, this) &&
            string.Equals(target.Definition.Id, "computer", StringComparison.Ordinal);

        public bool TryInteract(GameWorld world, Actor actor, Actor target)
        {
            if (!CanInteract(world, actor, target))
                return false;
            EffectUseCosts.TryConsume(actor, this);
            return true;
        }

        public override AccessoryEffect Clone() =>
            new TestUseComputerEffect(CostResourceTag, CostAmount);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }
}
