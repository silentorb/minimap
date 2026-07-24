using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

public class CellOccupancyTests
{
    [Fact]
    public void TryPlaceActor_rejects_occupied_or_missing_cell()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var def = new ActorDefinition("carrot");
        var cell = w.Grid.AllHexes().First();

        Assert.True(w.TryPlaceActor(cell, def));
        Assert.True(w.IsCellOccupied(cell));
        Assert.False(w.TryPlaceActor(cell, def));

        var outside = new HexAxial(99, 99);
        Assert.False(w.TryPlaceActor(outside, def));
    }

    [Fact]
    public void TryRemoveActorAt_removes_occupancy()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var def = new ActorDefinition("corn");
        var cell = w.Grid.AllHexes().First();
        Assert.True(w.TryPlaceActor(cell, def));
        Assert.True(w.TryRemoveActorAt(cell, out var removed));
        Assert.Equal("corn", removed!.Definition.Id);
        Assert.False(w.IsCellOccupied(cell));
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
