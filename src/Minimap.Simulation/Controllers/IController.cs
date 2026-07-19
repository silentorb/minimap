namespace Minimap.Simulation;

/// <summary>Drives a possessed <see cref="Character"/> (docs/technical/features/controllers.md).</summary>
public interface IController
{
    Character? Pawn { get; }
    void Possess(Character character);
    void Unpossess();
    void Tick(GameWorld world, float dt);
}
