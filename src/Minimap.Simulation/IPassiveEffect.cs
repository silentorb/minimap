using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Passive per-tick effect on an actor (e.g. energy drain, vitality).</summary>
public interface IPassiveEffect
{
    void Tick(Actor actor, float dt);
}
