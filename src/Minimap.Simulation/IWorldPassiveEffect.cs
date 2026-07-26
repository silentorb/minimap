namespace Minimap.Simulation;

/// <summary>World-aware passive tick for cell-anchored actors (e.g. auto-shoot).</summary>
public interface IWorldPassiveEffect
{
    void Tick(GameWorld world, Actor actor, float dt);
}
