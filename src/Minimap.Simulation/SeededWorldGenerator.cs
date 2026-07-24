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
                _ => CellType.Grass,
            });
        }
    }

    public static HexAxial[] PickGrassSpawns(HexGrid grid, int count, Random random)
    {
        var grass = grid.AllHexes().Where(h => grid.Get(h) == CellType.Grass).ToArray();
        if (grass.Length == 0)
            throw new InvalidOperationException("No grass cells for spawns.");

        var result = new HexAxial[count];
        for (var i = 0; i < count; i++)
            result[i] = grass[random.Next(grass.Length)];
        return result;
    }
}
