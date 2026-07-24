using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Modal ability effect that activates immediately (no placement preview).</summary>
public interface IInstantUseEffect
{
    /// <summary>Attempts to pay cost (if any) and apply. Returns false when refused.</summary>
    bool TryUse(Actor actor);
}
