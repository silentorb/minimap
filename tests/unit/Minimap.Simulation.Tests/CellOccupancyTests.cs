using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

public class CellOccupancyTests
{
    [Fact]
    public void TryPlaceObject_rejects_occupied_or_missing_cell()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var def = new PlacedObjectDefinition("carrot");
        var cell = w.Grid.AllHexes().First();

        Assert.True(w.TryPlaceObject(cell, def));
        Assert.True(w.IsCellOccupied(cell));
        Assert.False(w.TryPlaceObject(cell, def));

        var outside = new HexAxial(99, 99);
        Assert.False(w.TryPlaceObject(outside, def));
    }

    [Fact]
    public void TryRemoveObjectAt_removes_occupancy()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        var def = new PlacedObjectDefinition("corn");
        var cell = w.Grid.AllHexes().First();
        Assert.True(w.TryPlaceObject(cell, def));
        Assert.True(w.TryRemoveObjectAt(cell, out var removed));
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
