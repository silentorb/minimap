namespace Minimap.Simulation;

/// <summary>Converts a world-space move goal into a direction for <see cref="Character.MoveIntent"/>.</summary>
public interface IMoveSteering : IDisposable
{
    void SetGoal(SimVec2 worldGoal);
    void ClearGoal();
    SimVec2 SampleMoveIntent(Character pawn, float dt);
}
