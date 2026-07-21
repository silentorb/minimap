namespace Minimap.Simulation;

/// <summary>Shared wander policy: pause or pick a random floor world position.</summary>
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
    /// Returns null to pause; otherwise a world-space floor cell center.
    /// </summary>
    public static SimVec2? PickFloorGoalOrPause(GameWorld world, Random random)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(random);

        if (random.NextDouble() < PauseChance)
            return null;

        return TryPickRandomFloorWorld(world, random);
    }

    public static SimVec2? TryPickRandomFloorWorld(GameWorld world, Random random)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(random);

        var floors = new List<HexAxial>();
        foreach (var h in world.Grid.AllHexes())
        {
            if (world.Grid.Get(h) == CellType.Floor)
                floors.Add(h);
        }

        if (floors.Count == 0)
            return null;

        var hex = floors[random.Next(floors.Count)];
        return HexWorldLayout.ToWorld(hex, world.HexSize);
    }
}
