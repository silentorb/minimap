using Godot;
using Minimap.Simulation;

namespace Minimap.Simulation.Navigation;

/// <summary>Godot <see cref="NavigationAgent2D"/> crowd avoidance implementing <see cref="IMoveSteering"/>.</summary>
public sealed class GodotCrowdSteering : IMoveSteering
{
    private readonly Node2D _body;
    private readonly NavigationAgent2D _agent;
    private readonly float _maxSpeed;
    private readonly float _arriveDistance;
    private SimVec2? _goal;
    private Vector2 _safeVelocity;
    private bool _disposed;

    public GodotCrowdSteering(
        Node agentParent,
        float agentRadius,
        float maxSpeed,
        float arriveDistance = DirectMoveSteering.DefaultArriveDistance)
    {
        ArgumentNullException.ThrowIfNull(agentParent);
        _maxSpeed = Math.Max(1f, maxSpeed);
        _arriveDistance = Math.Max(0.1f, arriveDistance);

        // NavigationAgent2D is a Node (not Node2D); parent it under a body we can position.
        _body = new Node2D { Name = "CrowdSteeringBody" };
        agentParent.AddChild(_body);

        _agent = new NavigationAgent2D
        {
            Name = "CrowdSteeringAgent",
            AvoidanceEnabled = true,
            Radius = Math.Max(1f, agentRadius),
            MaxSpeed = _maxSpeed,
            PathDesiredDistance = 4f,
            TargetDesiredDistance = _arriveDistance,
        };
        _body.AddChild(_agent);
        _agent.VelocityComputed += OnVelocityComputed;
    }

    public void SetGoal(SimVec2 worldGoal)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _goal = worldGoal;
        _agent.TargetPosition = new Vector2(worldGoal.X, worldGoal.Y);
    }

    public void ClearGoal()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _goal = null;
        _safeVelocity = Vector2.Zero;
        if (GodotObject.IsInstanceValid(_agent) && GodotObject.IsInstanceValid(_body))
        {
            _agent.Velocity = Vector2.Zero;
            _agent.TargetPosition = _body.GlobalPosition;
        }
    }

    public SimVec2 SampleMoveIntent(Actor pawn, float dt)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(pawn);

        if (!GodotObject.IsInstanceValid(_agent) || !GodotObject.IsInstanceValid(_body))
            return SimVec2.Zero;

        _body.GlobalPosition = new Vector2(pawn.Position.X, pawn.Position.Y);

        if (_goal is not { } goal)
        {
            _agent.Velocity = Vector2.Zero;
            return SimVec2.Zero;
        }

        var toGoal = goal - pawn.Position;
        if (toGoal.LengthSquared <= _arriveDistance * _arriveDistance)
        {
            _agent.Velocity = Vector2.Zero;
            return SimVec2.Zero;
        }

        var next = _agent.GetNextPathPosition();
        var desired = next - _body.GlobalPosition;
        if (desired.LengthSquared() < 1e-8f)
        {
            _agent.Velocity = Vector2.Zero;
            return SimVec2.Zero;
        }

        _agent.Velocity = desired.Normalized() * _maxSpeed;

        if (_safeVelocity.LengthSquared() < 1e-8f)
        {
            var fallback = desired.Normalized();
            return new SimVec2(fallback.X, fallback.Y);
        }

        var safe = _safeVelocity.Normalized();
        return new SimVec2(safe.X, safe.Y);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _goal = null;
        if (GodotObject.IsInstanceValid(_agent))
            _agent.VelocityComputed -= OnVelocityComputed;
        if (GodotObject.IsInstanceValid(_body))
            _body.QueueFree();
    }

    private void OnVelocityComputed(Vector2 safeVelocity) => _safeVelocity = safeVelocity;
}
