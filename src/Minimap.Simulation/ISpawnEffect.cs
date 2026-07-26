namespace Minimap.Simulation;

/// <summary>World-aware passive that periodically emits characters near a cell actor.</summary>
public interface ISpawnEffect
{
    float IntervalSeconds { get; }

    int Volume { get; }

    void Tick(GameWorld world, Actor actor, float dt);
}
