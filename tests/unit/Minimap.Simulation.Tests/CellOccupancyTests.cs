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
        var def = new ActorDefinition("carrot_growing");
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
        var def = new ActorDefinition("corn_growing");
        var cell = w.Grid.AllHexes().First();
        Assert.True(w.TryPlaceActor(cell, def));
        Assert.True(w.TryRemoveActorAt(cell, out var removed));
        Assert.Equal("corn_growing", removed!.Definition.Id);
        Assert.False(w.IsCellOccupied(cell));
        Assert.DoesNotContain(removed, w.Actors);
    }

    [Fact]
    public void TryPlaceActor_adds_to_actors_collection_and_occupancy()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var def = new ActorDefinition("carrot_growing");
        var cell = w.Grid.AllHexes().First();

        Assert.True(w.TryPlaceActor(cell, def));
        Assert.True(w.TryGetActorAt(cell, out var placed));
        Assert.Contains(placed!, w.Actors);
        Assert.Same(placed, w.CellActors[cell]);
    }

    [Fact]
    public void AddActor_is_in_actors_but_not_occupancy()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var actor = w.AddActor(1, SimVec2.Zero);
        Assert.Contains(actor, w.Actors);
        Assert.Null(actor.Cell);
        Assert.Empty(w.CellActors);
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
