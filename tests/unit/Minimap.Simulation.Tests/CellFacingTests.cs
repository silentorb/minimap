using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

public class CellFacingTests
{
    [Fact]
    public void CellInFront_uses_facing_neighbor()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(4, 4, 7, gen);
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnCharacterDefinition(TestContent.Bare);
        var c = w.AddCharacter(1, HexWorldLayout.ToWorld(new HexAxial(0, 0), w.HexSize), TestContent.Bare);
        c.Facing = new SimVec2(1f, 0f);

        var front = CellFacing.CellInFront(c, w.HexSize);
        var origin = HexWorldLayout.WorldToAxial(c.Position, w.HexSize);
        Assert.Equal(origin + CellFacing.NeighborOffsetForFacing(c.Facing), front);
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
