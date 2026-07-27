namespace Minimap.Simulation;

/// <summary>Converts a world-space move goal into a direction for <see cref="Actor.MoveIntent"/>.</summary>
public interface IMoveSteering : IDisposable
{
    void SetGoal(SimVec2 worldGoal);
    void ClearGoal();
    SimVec2 SampleMoveIntent(Actor pawn, float dt);
}
