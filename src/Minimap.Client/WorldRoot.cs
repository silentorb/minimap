using Godot;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Root scene: owns <see cref="GameWorld"/>, drives evolution timer, syncs entity visuals.</summary>
public partial class WorldRoot : Node2D
{
    [Export] public int GridRadiusX { get; set; } = 8;
    [Export] public int GridRadiusY { get; set; } = 6;
    [Export] public int WorldSeed { get; set; } = 42;
    [Export] public float HexSize { get; set; } = HexLayout.DefaultHexSize;
    [Export] public int PlayerFactionId { get; set; } = 1;
    [Export] public int RivalFactionId { get; set; } = 2;
    [Export] public int AiPerFaction { get; set; } = 3;

    private GameWorld? _world;
    private Random _rng = new();
    private Node2D? _hexLayer;
    private Node2D? _playerLayer;
    private Node2D? _missileLayer;
    private PackedScene? _hexScene;
    private PackedScene? _playerScene;
    private readonly Dictionary<HexAxial, Node2D> _hexNodes = new();
    private readonly Dictionary<int, Node2D> _characterNodes = new();
    private readonly Dictionary<int, Node2D> _missileNodes = new();
    private readonly HashSet<Key> _heldKeys = new();

    public override void _Ready()
    {
        _rng = new Random(WorldSeed);
        var spawn = new SpawnConfig
        {
            PlayerFactionId = PlayerFactionId,
            RivalFactionId = RivalFactionId,
            AiPerFaction = AiPerFaction,
        };
        _world = GameWorld.Create(GridRadiusX, GridRadiusY, WorldSeed, hexSize: HexSize, spawn: spawn);
        _hexLayer = GetNode<Node2D>("HexLayer");
        _playerLayer = GetNode<Node2D>("PlayerLayer");
        _missileLayer = GetNodeOrNull<Node2D>("MissileLayer");
        if (_missileLayer is null)
        {
            _missileLayer = new Node2D { Name = "MissileLayer" };
            AddChild(_missileLayer);
        }

        _hexScene = GD.Load<PackedScene>("res://entities/hex_cell.tscn");
        _playerScene = GD.Load<PackedScene>("res://entities/player_visual.tscn");
        GetNode<Godot.Timer>("EvolutionTimer").Timeout += OnEvolutionTick;
        SyncHexes();
        SyncCharacters();
        SyncMissiles();
        RecenterCamera();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey k)
            return;
        if (k.Echo)
            return;

        if (k.Pressed)
            _heldKeys.Add(k.Keycode);
        else
            _heldKeys.Remove(k.Keycode);

        if (IsMovementKey(k.Keycode))
            GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        if (_world is null)
            return;

        var input = ReadScreenAxisInput();
        _world.PlayerController?.SetMoveInput(input);
        _world.Tick((float)delta);
        SyncCharacters();
        SyncMissiles();
    }

    private void OnEvolutionTick()
    {
        if (_world is null)
            return;
        WorldEvolution.Tick(_world, _rng);
        SyncHexes();
        SyncCharacters();
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

    private void SyncCharacters()
    {
        if (_world is null || _playerLayer is null || _playerScene is null)
            return;

        var live = new HashSet<int>();
        foreach (var character in _world.Characters)
        {
            live.Add(character.Id);
            if (!_characterNodes.TryGetValue(character.Id, out var node))
            {
                node = _playerScene.Instantiate<Node2D>();
                _playerLayer.AddChild(node);
                _characterNodes[character.Id] = node;
                var cr = node.GetNode<ColorRect>("ColorRect");
                cr.Color = FactionColor(character.FactionId, character.Id);
            }

            var p = character.Position;
            node.Position = new Vector2(p.X, p.Y);
            node.Visible = true;
            node.ZIndex = 2;
        }

        foreach (var id in _characterNodes.Keys.ToList())
        {
            if (live.Contains(id))
                continue;
            _characterNodes[id].QueueFree();
            _characterNodes.Remove(id);
        }
    }

    private void SyncMissiles()
    {
        if (_world is null || _missileLayer is null)
            return;

        var live = new HashSet<int>();
        foreach (var missile in _world.Missiles)
        {
            live.Add(missile.Id);
            if (!_missileNodes.TryGetValue(missile.Id, out var node))
            {
                node = new Node2D();
                var rect = new ColorRect
                {
                    Color = new Color(1f, 0.9f, 0.3f),
                    OffsetLeft = -4,
                    OffsetTop = -4,
                    OffsetRight = 4,
                    OffsetBottom = 4,
                };
                node.AddChild(rect);
                _missileLayer.AddChild(node);
                _missileNodes[missile.Id] = node;
            }

            node.Position = new Vector2(missile.Position.X, missile.Position.Y);
            node.ZIndex = 3;
        }

        foreach (var id in _missileNodes.Keys.ToList())
        {
            if (live.Contains(id))
                continue;
            _missileNodes[id].QueueFree();
            _missileNodes.Remove(id);
        }
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

    private SimVec2 ReadScreenAxisInput()
    {
        var x = 0f;
        var y = 0f;
        if (_heldKeys.Contains(Key.Right) || Input.IsKeyPressed(Key.Right))
            x += 1f;
        if (_heldKeys.Contains(Key.Left) || Input.IsKeyPressed(Key.Left))
            x -= 1f;
        if (_heldKeys.Contains(Key.Down) || Input.IsKeyPressed(Key.Down))
            y += 1f;
        if (_heldKeys.Contains(Key.Up) || Input.IsKeyPressed(Key.Up))
            y -= 1f;
        return new SimVec2(x, y);
    }

    private static bool IsMovementKey(Key key) =>
        key is Key.Up or Key.Down or Key.Left or Key.Right;

    private static Color ColorFor(CellType t) =>
        t switch
        {
            CellType.Floor => new Color(0.35f, 0.42f, 0.38f),
            CellType.Wall => new Color(0.18f, 0.16f, 0.22f),
            CellType.Hazard => new Color(0.75f, 0.2f, 0.35f),
            _ => new Color(0.1f, 0.1f, 0.12f),
        };

    private static Color FactionColor(int factionId, int characterId)
    {
        var baseColor = factionId switch
        {
            1 => new Color(0.35f, 0.75f, 1f),
            2 => new Color(1f, 0.45f, 0.35f),
            _ => new Color(0.7f, 0.7f, 0.7f),
        };
        var shade = (characterId % 3) * 0.06f;
        return new Color(
            Math.Clamp(baseColor.R + shade, 0f, 1f),
            Math.Clamp(baseColor.G + shade * 0.5f, 0f, 1f),
            Math.Clamp(baseColor.B - shade * 0.3f, 0f, 1f));
    }

    /// <summary>Automation: set held state for a movement key (continuous motion while pressed).</summary>
    public void SetMovementKeyState(Key key, bool pressed)
    {
        if (!IsMovementKey(key))
            return;
        if (pressed)
            _heldKeys.Add(key);
        else
            _heldKeys.Remove(key);
    }

    public int HexLayerChildCount => _hexLayer?.GetChildCount() ?? 0;

    public int PlayerLayerChildCount => _playerLayer?.GetChildCount() ?? 0;

    public Vector2? TryGetPlayerPosition(int playerIndex)
    {
        if (_world?.PlayerController?.Pawn is { } pawn && playerIndex == 0)
            return new Vector2(pawn.Position.X, pawn.Position.Y);

        if (_characterNodes.Count == 0)
            return null;
        // Fallback: first visual by sorted id
        var first = _characterNodes.OrderBy(kv => kv.Key).FirstOrDefault();
        return first.Value?.Position;
    }
}
