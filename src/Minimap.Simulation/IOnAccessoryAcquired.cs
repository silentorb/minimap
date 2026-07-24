using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Effect that runs when its accessory is added to an actor.</summary>
public interface IOnAccessoryAcquired
{
    void OnAcquired(Actor actor);
}
