using Minimap.Simulation;

namespace Minimap.Functional.Tests;

/// <summary>Deterministic generator: all floors (spawns chosen by GameWorld).</summary>
internal sealed class FixedLayoutGenerator : IWorldGenerator
{
    public void GenerateTerrain(HexGrid grid, Random random)
    {
        foreach (var h in grid.AllHexes())
            grid.Set(h, CellType.Floor);
    }
}
