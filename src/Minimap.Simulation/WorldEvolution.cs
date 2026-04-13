namespace Minimap.Simulation;

/// <summary>Mutates the grid over time (supernatural creep / office decay vibe).</summary>
public static class WorldEvolution
{
    public static void Tick(GameWorld world, Random random)
    {
        world.AdvanceTick();
        var list = world.Grid.AllHexes().ToList();
        if (list.Count == 0)
            return;

        var h = list[random.Next(list.Count)];
        var t = world.Grid.Get(h);
        foreach (var n in h.Neighbors())
        {
            if (!world.Grid.Contains(n))
                continue;
            if (random.NextDouble() > 0.4)
                continue;
            world.Grid.Set(n, Spread(t, random));
        }
    }

    private static CellType Spread(CellType from, Random random)
    {
        return from switch
        {
            CellType.Hazard => random.NextDouble() < 0.5 ? CellType.Hazard : CellType.Floor,
            CellType.Wall => random.NextDouble() < 0.15 ? CellType.Floor : CellType.Wall,
            CellType.Floor => random.NextDouble() < 0.12 ? CellType.Hazard : CellType.Floor,
            _ => CellType.Floor,
        };
    }
}
