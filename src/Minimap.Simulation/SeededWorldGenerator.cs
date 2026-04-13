namespace Minimap.Simulation;

/// <summary>Weighted random terrain plus corner-ish spawns for up to four players.</summary>
public sealed class SeededWorldGenerator : IWorldGenerator
{
    public void Generate(HexGrid grid, Span<PlayerSlot> playersOut, Random random)
    {
        foreach (var h in grid.AllHexes())
        {
            var roll = random.NextDouble();
            grid.Set(h, roll switch
            {
                < 0.08 => CellType.Wall,
                < 0.18 => CellType.Hazard,
                _ => CellType.Floor,
            });
        }

        var spawns = PickSpawnHexes(grid, playersOut.Length, random);
        for (var i = 0; i < playersOut.Length; i++)
            playersOut[i] = new PlayerSlot(i, spawns[i]);
    }

    private static HexAxial[] PickSpawnHexes(HexGrid grid, int count, Random random)
    {
        var floors = grid.AllHexes().Where(h => grid.Get(h) == CellType.Floor).ToArray();
        if (floors.Length == 0)
            throw new InvalidOperationException("No floor cells for spawns.");

        var result = new HexAxial[count];
        for (var i = 0; i < count; i++)
            result[i] = floors[random.Next(floors.Length)];
        return result;
    }
}
