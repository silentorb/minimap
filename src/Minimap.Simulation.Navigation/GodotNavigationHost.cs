using Godot;
using Minimap.Simulation;

namespace Minimap.Simulation.Navigation;

/// <summary>Owns a <see cref="NavigationRegion2D"/> baked from floor hexes and creates crowd steerings.</summary>
public sealed class GodotNavigationHost : IDisposable
{
    private readonly Node2D _parent;
    private readonly float _agentRadius;
    private readonly float _maxSpeed;
    private readonly NavigationRegion2D _region;
    private bool _disposed;

    private GodotNavigationHost(Node2D parent, float agentRadius, float maxSpeed)
    {
        _parent = parent;
        _agentRadius = Math.Max(1f, agentRadius);
        _maxSpeed = Math.Max(1f, maxSpeed);
        _region = new NavigationRegion2D { Name = "HexNavigationRegion" };
        _parent.AddChild(_region);
    }

    public NavigationRegion2D Region => _region;

    public static GodotNavigationHost Create(
        Node2D parent,
        GameWorld world,
        float agentRadius,
        float maxSpeed = CombatTuning.MoveSpeed)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(world);

        var host = new GodotNavigationHost(parent, agentRadius, maxSpeed);
        host.Rebuild(world);
        return host;
    }

    public void Rebuild(GameWorld world)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(world);

        var polygon = new NavigationPolygon();
        polygon.AgentRadius = _agentRadius;

        var source = new NavigationMeshSourceGeometryData2D();
        var local = HexWorldLayout.PointyHexVertices(world.HexSize);
        var any = false;
        foreach (var h in world.Grid.AllHexes())
        {
            if (world.Grid.Get(h) != CellType.Grass)
                continue;

            any = true;
            var center = HexWorldLayout.ToWorld(h, world.HexSize);
            var outline = new Vector2[local.Length];
            for (var i = 0; i < local.Length; i++)
                outline[i] = new Vector2(center.X + local[i].X, center.Y + local[i].Y);
            source.AddTraversableOutline(outline);
        }

        if (any)
            NavigationServer2D.BakeFromSourceGeometryData(polygon, source);

        _region.NavigationPolygon = polygon;
    }

    public IMoveSteering CreateCrowdSteering()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new GodotCrowdSteering(_region, _agentRadius, _maxSpeed);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (GodotObject.IsInstanceValid(_region))
            _region.QueueFree();
    }
}
