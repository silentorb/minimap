using Minimap.Simulation;

namespace Minimap.Functional.Tests;

/// <summary>Deterministic generator: all floors and fixed spawn hexes (for functional movement scenarios).</summary>
internal sealed class FixedLayoutGenerator : IWorldGenerator
{
    private readonly HexAxial[] _spawns;

    public FixedLayoutGenerator(params HexAxial[] spawns) => _spawns = spawns;

    public void Generate(HexGrid grid, Span<PlayerSlot> playersOut, Random random)
    {
        foreach (var h in grid.AllHexes())
            grid.Set(h, CellType.Floor);
        for (var i = 0; i < playersOut.Length; i++)
            playersOut[i] = new PlayerSlot(i, _spawns[i]);
    }
}
