using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class MovementEnergyTests
{
    [Fact]
    public void Distance_drain_subtracts_energy_after_120_units()
    {
        var world = GameWorld.Create(8, 8, 1, new AllGrassGenerator());
        world.ApplyGameContent(TestContent.Content);
        var pawn = world.AddCharacter(1, SimVec2.Zero, TestContent.Bare);
        pawn.AddAccessory(new AccessoryDefinition(
            "movement_energy",
            [new TestDrainResourceByDistanceEffect(TestContent.EnergyResource.Tag, 120f)],
            activation: AccessoryActivation.None).CreateInstance());

        var before = pawn.Energy;
        world.TickCharacterPassives(0.016f);
        Assert.Equal(before, pawn.Energy);

        pawn.Position = new SimVec2(120f, 0f);
        world.TickCharacterPassives(0.016f);
        Assert.Equal(before - 1, pawn.Energy);
    }

    [Fact]
    public void Distance_drain_idle_does_not_spend_energy()
    {
        var world = GameWorld.Create(2, 1);
        world.ApplyGameContent(TestContent.Content);
        var pawn = world.AddCharacter(1, SimVec2.Zero, TestContent.Bare);
        pawn.AddAccessory(new AccessoryDefinition(
            "movement_energy",
            [new TestDrainResourceByDistanceEffect(TestContent.EnergyResource.Tag, 120f)],
            activation: AccessoryActivation.None).CreateInstance());

        var before = pawn.Energy;
        world.TickCharacterPassives(0.016f);
        world.TickCharacterPassives(1f);
        world.TickCharacterPassives(1f);
        Assert.Equal(before, pawn.Energy);
    }

    private sealed class TestDrainResourceByDistanceEffect : AccessoryEffect, IPassiveEffect
    {
        private float _accumulator;
        private SimVec2? _lastPosition;

        public TestDrainResourceByDistanceEffect(TagId resourceTag, float unitsPerAmount)
        {
            ResourceTag = resourceTag;
            UnitsPerAmount = unitsPerAmount;
        }

        public TagId ResourceTag { get; }
        public float UnitsPerAmount { get; }

        public void Tick(Actor actor, float dt)
        {
            if (actor is not Character character)
                return;

            var position = character.Position;
            if (_lastPosition is not SimVec2 previous)
            {
                _lastPosition = position;
                return;
            }

            var delta = position - previous;
            _lastPosition = position;
            var distance = MathF.Sqrt(delta.LengthSquared);
            if (distance <= 0f)
                return;

            _accumulator += distance;
            var whole = (int)(_accumulator / UnitsPerAmount);
            if (whole <= 0)
                return;

            _accumulator -= whole * UnitsPerAmount;
            actor.AddResource(ResourceTag, -whole);
        }

        public override AccessoryEffect Clone() =>
            new TestDrainResourceByDistanceEffect(ResourceTag, UnitsPerAmount);
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
