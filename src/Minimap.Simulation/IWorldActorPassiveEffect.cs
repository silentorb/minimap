namespace Minimap.Simulation;

/// <summary>World-aware passive tick for actors (e.g. companion spawn).</summary>
public interface IWorldActorPassiveEffect
{
    void Tick(GameWorld world, Actor actor, float dt);
}
