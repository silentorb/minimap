using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Passive grow timer on food actors; exposes maturity and harvest yield.</summary>
public interface IGrowEffect
{
    bool IsMature { get; }

    TagId? YieldResourceTag { get; }

    int YieldAmount { get; }

    /// <summary>When set, harvest/emerge spawns this character id instead of granting yield.</summary>
    string? EmergeCharacterId { get; }

    float EmergeAfterMatureSeconds { get; }

    void Tick(GameWorld world, Actor actor, float dt);

    /// <summary>
    /// If post-mature emerge is due (or forced by harvest), removes the cell actor and spawns
    /// the emerge character. Harvest may call this even before <see cref="EmergeAfterMatureSeconds"/>.
    /// </summary>
    bool TryEmerge(GameWorld world, Actor actor);
}
