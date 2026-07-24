using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Passive grow timer on food actors; exposes maturity and harvest yield.</summary>
public interface IGrowEffect
{
    bool IsMature { get; }

    TagId? YieldResourceTag { get; }

    int YieldAmount { get; }

    void Tick(Actor actor, float dt);
}
