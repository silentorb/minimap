namespace Minimap.Simulation;

/// <summary>Weighted random terrain for the playable hex map.</summary>
public sealed class SeededWorldGenerator : IWorldGenerator
{
    public void GenerateTerrain(HexGrid grid, Random random)
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
    }

    public static HexAxial[] PickFloorSpawns(HexGrid grid, int count, Random random)
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
