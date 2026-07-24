namespace Minimap.Simulation;

/// <summary>Shared wander policy: pause or pick a random grass world position.</summary>
public static class AiWanderGoals
{
    public const float RetargetMinSeconds = 0.6f;
    public const float RetargetMaxSeconds = 1.8f;
    public const double PauseChance = 0.2;

    public static float NextRetargetDelay(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return RetargetMinSeconds
            + (float)random.NextDouble() * (RetargetMaxSeconds - RetargetMinSeconds);
    }

    /// <summary>
    /// Returns null to pause; otherwise a world-space grass cell center.
    /// </summary>
    public static SimVec2? PickGrassGoalOrPause(GameWorld world, Random random)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(random);

        if (random.NextDouble() < PauseChance)
            return null;

        return TryPickRandomGrassWorld(world, random);
    }

    public static SimVec2? TryPickRandomGrassWorld(GameWorld world, Random random)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(random);

        var grass = new List<HexAxial>();
        foreach (var h in world.Grid.AllHexes())
        {
            if (world.Grid.Get(h) == CellType.Grass)
                grass.Add(h);
        }

        if (grass.Count == 0)
            return null;

        var hex = grass[random.Next(grass.Count)];
        return HexWorldLayout.ToWorld(hex, world.HexSize);
    }
}
