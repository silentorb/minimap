using Godot;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Root scene: owns <see cref="GameWorld"/>, drives evolution timer, syncs entity visuals.</summary>
public partial class WorldRoot : Node2D
{
    [Export] public int GridRadius { get; set; } = 4;
    [Export] public int PlayerCount { get; set; } = 4;
    [Export] public int WorldSeed { get; set; } = 42;
    [Export] public float HexSize { get; set; } = HexLayout.DefaultHexSize;

    private GameWorld? _world;
    private Random _rng = new();
    private Node2D? _hexLayer;
    private Node2D? _playerLayer;
    private PackedScene? _hexScene;
    private PackedScene? _playerScene;
    private readonly Dictionary<HexAxial, Node2D> _hexNodes = new();
    private readonly List<Node2D> _playerNodes = new();

    public override void _Ready()
    {
        _rng = new Random(WorldSeed);
        _world = GameWorld.Create(GridRadius, PlayerCount, WorldSeed);
        _hexLayer = GetNode<Node2D>("HexLayer");
        _playerLayer = GetNode<Node2D>("PlayerLayer");
        _hexScene = GD.Load<PackedScene>("res://entities/hex_cell.tscn");
        _playerScene = GD.Load<PackedScene>("res://entities/player_visual.tscn");
        GetNode<Godot.Timer>("EvolutionTimer").Timeout += OnEvolutionTick;
        SyncHexes();
        SyncPlayers();
        RecenterCamera();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_world is null)
            return;
        if (@event is not InputEventKey k || !k.Pressed || k.Echo)
            return;

        // Neighbor indices match HexAxial.NeighborOffsets: E, NE, NW, W, SW, SE
        var moved = k.Keycode switch
        {
            Key.Right => _world.TryMovePlayer(0, 0),
            Key.Up => _world.TryMovePlayer(0, k.ShiftPressed ? 1 : 2),
            Key.Left => _world.TryMovePlayer(0, 3),
            Key.Down => _world.TryMovePlayer(0, k.ShiftPressed ? 4 : 5),
            _ => false,
        };

        if (moved)
        {
            SyncPlayers();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnEvolutionTick()
    {
        if (_world is null)
            return;
        WorldEvolution.Tick(_world, _rng);
        SyncHexes();
        SyncPlayers();
    }

    private void SyncHexes()
    {
        if (_world is null || _hexLayer is null || _hexScene is null)
            return;
        var polyTemplate = HexLayout.PointyHexPolygon(HexSize);
        foreach (var h in _world.Grid.AllHexes())
        {
            if (!_hexNodes.TryGetValue(h, out var node))
            {
                node = _hexScene.Instantiate<Node2D>();
                _hexLayer.AddChild(node);
                _hexNodes[h] = node;
            }

            node.Position = HexLayout.ToWorld(h, HexSize);
            var poly = node.GetNode<Polygon2D>("Polygon2D");
            poly.Polygon = polyTemplate;
            poly.Color = ColorFor(_world.Grid.Get(h));
        }
    }

    private void SyncPlayers()
    {
        if (_world is null || _playerLayer is null || _playerScene is null)
            return;
        while (_playerNodes.Count < _world.Players.Length)
        {
            var n = _playerScene.Instantiate<Node2D>();
            _playerLayer.AddChild(n);
            _playerNodes.Add(n);
            var cr = n.GetNode<ColorRect>("ColorRect");
            cr.Color = PlayerColor(_playerNodes.Count - 1);
        }

        for (var i = 0; i < _world.Players.Length; i++)
        {
            var node = _playerNodes[i];
            node.Position = HexLayout.ToWorld(_world.Players[i].Position, HexSize);
            node.Visible = true;
            node.ZIndex = 2;
        }

        for (var i = _world.Players.Length; i < _playerNodes.Count; i++)
            _playerNodes[i].Visible = false;
    }

    private void RecenterCamera()
    {
        if (_world is null)
            return;
        var cam = GetNode<Camera2D>("Camera2D");
        Vector2 sum = Vector2.Zero;
        var n = 0;
        foreach (var h in _world.Grid.AllHexes())
        {
            sum += HexLayout.ToWorld(h, HexSize);
            n++;
        }

        if (n > 0)
            cam.Position = sum / n;
    }

    private static Color ColorFor(CellType t) =>
        t switch
        {
            CellType.Floor => new Color(0.35f, 0.42f, 0.38f),
            CellType.Wall => new Color(0.18f, 0.16f, 0.22f),
            CellType.Hazard => new Color(0.75f, 0.2f, 0.35f),
            _ => new Color(0.1f, 0.1f, 0.12f),
        };

    private static Color PlayerColor(int index) =>
        index switch
        {
            0 => new Color(0.35f, 0.75f, 1f),
            1 => new Color(1f, 0.55f, 0.2f),
            2 => new Color(0.55f, 1f, 0.35f),
            3 => new Color(0.85f, 0.45f, 1f),
            _ => Colors.White,
        };
}
