using Minimap.Simulation;

namespace Minimap.Functional.Tests;

/// <summary>All floors except an east wall at axial (1,0) for collision journeys.</summary>
internal sealed class CorridorWithEastWallGenerator : IWorldGenerator
{
    public void GenerateTerrain(HexGrid grid, Random random)
    {
        foreach (var h in grid.AllHexes())
            grid.Set(h, CellType.Floor);
        grid.Set(new HexAxial(1, 0), CellType.Wall);
    }
}
