namespace Minimap.Simulation;

/// <summary>World-aware passive tick for characters (e.g. companion spawn).</summary>
public interface IWorldCharacterPassiveEffect
{
    void Tick(GameWorld world, Character character, float dt);
}
