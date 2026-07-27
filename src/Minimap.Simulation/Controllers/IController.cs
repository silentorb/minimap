namespace Minimap.Simulation;

/// <summary>Drives a possessed <see cref="Actor"/> (docs/technical/features/gameplay/controllers.md).</summary>
public interface IController
{
    Actor? Pawn { get; }
    void Possess(Actor actor);
    void Unpossess();
    void Tick(GameWorld world, float dt);
}
