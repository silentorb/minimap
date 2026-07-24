using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Test double for ICellPlacementEffect (CompuQuest effect stays in extension).</summary>
internal sealed class TestPlaceEffect : AccessoryEffect, ICellPlacementEffect
{
    private readonly ActorDefinition _definition;

    public TestPlaceEffect(ActorDefinition definition)
    {
        _definition = definition;
    }

    public int PlaceAttempts { get; private set; }

    public bool CanPlace(GameWorld world, Character placer, HexAxial cell)
    {
        if (!world.Grid.Contains(cell))
            return false;
        if (world.Grid.Get(cell) != CellType.Grass)
            return false;
        if (world.IsCellOccupied(cell))
            return false;
        return true;
    }

    public bool TryPlace(GameWorld world, Character placer, HexAxial cell, Random random)
    {
        PlaceAttempts++;
        if (!CanPlace(world, placer, cell))
            return false;
        return world.TryPlaceActor(cell, _definition);
    }

    public override AccessoryEffect Clone() => new TestPlaceEffect(_definition);
}

public class CellPlacementEffectTests
{
    [Fact]
    public void Place_effect_requires_unoccupied_grass()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnCharacterDefinition(TestContent.Bare);
        var veg = new ActorDefinition("carrot");
        w.SetActorDefinitions([veg]);
        var effect = new TestPlaceEffect(veg);
        var placer = w.AddCharacter(1, SimVec2.Zero, TestContent.Bare);
        var grass = w.Grid.AllHexes().First(h => w.Grid.Get(h) == CellType.Grass);

        Assert.True(effect.TryPlace(w, placer, grass, new Random(1)));
        Assert.False(effect.TryPlace(w, placer, grass, new Random(1)));

        var wall = w.Grid.AllHexes().First();
        w.Grid.Set(wall, CellType.Wall);
        Assert.False(effect.CanPlace(w, placer, wall));
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
