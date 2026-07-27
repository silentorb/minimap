namespace Minimap.Simulation;

/// <summary>Headless steering: move directly toward the goal (no pathfinding).</summary>
public sealed class DirectMoveSteering : IMoveSteering
{
    public const float DefaultArriveDistance = 8f;

    private SimVec2? _goal;
    private readonly float _arriveDistance;

    public DirectMoveSteering(float arriveDistance = DefaultArriveDistance)
    {
        _arriveDistance = Math.Max(0.1f, arriveDistance);
    }

    public void SetGoal(SimVec2 worldGoal) => _goal = worldGoal;

    public void ClearGoal() => _goal = null;

    public SimVec2 SampleMoveIntent(Actor pawn, float dt)
    {
        ArgumentNullException.ThrowIfNull(pawn);
        if (_goal is not { } goal)
            return SimVec2.Zero;

        var delta = goal - pawn.Position;
        if (delta.LengthSquared <= _arriveDistance * _arriveDistance)
            return SimVec2.Zero;

        return delta.Normalized();
    }

    public void Dispose()
    {
        _goal = null;
    }
}
