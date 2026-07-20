using Godot;
using Minimap.Automation;
using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>World visuals + keyboard capture. Does not own or tick the simulation.</summary>
public partial class WorldView : Node2D, IMovementKeyTarget
{
    [Export] public float HexSize { get; set; } = HexLayout.DefaultHexSize;

    private GameWorld? _world;
    private Random _rng = new(1);
    private Node2D? _hexLayer;
    private Node2D? _spawnerLayer;
    private Node2D? _playerLayer;
    private Node2D? _missileLayer;
    private PackedScene? _hexScene;
    private PackedScene? _spawnerScene;
    private PackedScene? _playerScene;
    private readonly Dictionary<HexAxial, Node2D> _hexNodes = new();
    private readonly Dictionary<int, Node2D> _spawnerNodes = new();
    private readonly Dictionary<int, Node2D> _characterNodes = new();
    private readonly Dictionary<int, Node2D> _missileNodes = new();
    private readonly HashSet<Key> _heldKeys = new();
    private readonly List<Character> _humanPawns = new();

    public void Bind(GameWorld world, Random rng, IReadOnlyList<Character> humanPawns)
    {
        _world = world;
        _rng = rng;
        _humanPawns.Clear();
        _humanPawns.AddRange(humanPawns);

        _hexLayer = GetNode<Node2D>("HexLayer");
        _spawnerLayer = GetNodeOrNull<Node2D>("SpawnerLayer");
        if (_spawnerLayer is null)
        {
            _spawnerLayer = new Node2D { Name = "SpawnerLayer" };
            AddChild(_spawnerLayer);
        }

        _playerLayer = GetNode<Node2D>("PlayerLayer");
        _missileLayer = GetNodeOrNull<Node2D>("MissileLayer");
        if (_missileLayer is null)
        {
            _missileLayer = new Node2D { Name = "MissileLayer" };
            AddChild(_missileLayer);
        }

        _hexScene = GD.Load<PackedScene>("res://entities/hex_cell.tscn");
        _spawnerScene = GD.Load<PackedScene>("res://entities/wave_spawner_visual.tscn");
        _playerScene = GD.Load<PackedScene>("res://entities/player_visual.tscn");
        var timer = GetNodeOrNull<Godot.Timer>("EvolutionTimer");
        if (timer is not null)
            timer.Timeout += OnEvolutionTick;

        SyncAll();
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

    public void SyncFrame()
    {
        SyncCharacters();
        SyncMissiles();
        SyncSpawners();
    }

    public void OnLevelRegenerated(IReadOnlyList<Character> humanPawns)
    {
        _humanPawns.Clear();
        _humanPawns.AddRange(humanPawns);
        SyncAll();
        RecenterCamera();
    }

    public SimVec2 ReadMoveInput() => ReadScreenAxisInput();

    private void OnEvolutionTick()
    {
        if (_world is null)
            return;
        WorldEvolution.Tick(_world, _rng);
        SyncHexes();
        SyncCharacters();
    }

    private void SyncAll()
    {
        SyncHexes();
        SyncSpawners();
        SyncCharacters();
        SyncMissiles();
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

    private void SyncSpawners()
    {
        if (_world is null || _spawnerLayer is null || _spawnerScene is null)
            return;

        var live = new HashSet<int>();
        foreach (var spawner in _world.Spawners)
        {
            live.Add(spawner.Id);
            if (!_spawnerNodes.TryGetValue(spawner.Id, out var node))
            {
                node = _spawnerScene.Instantiate<Node2D>();
                _spawnerLayer.AddChild(node);
                _spawnerNodes[spawner.Id] = node;
            }

            var worldPos = HexLayout.ToWorld(spawner.Position, HexSize);
            node.Position = new Vector2(worldPos.X, worldPos.Y);
            node.ZIndex = 1;
        }

        foreach (var id in _spawnerNodes.Keys.ToList())
        {
            if (live.Contains(id))
                continue;
            _spawnerNodes[id].QueueFree();
            _spawnerNodes.Remove(id);
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
        if (playerIndex >= 0 && playerIndex < _humanPawns.Count)
        {
            var pawn = _humanPawns[playerIndex];
            if (_world is not null && _world.Characters.Contains(pawn))
                return new Vector2(pawn.Position.X, pawn.Position.Y);
        }

        if (_characterNodes.Count == 0)
            return null;
        var first = _characterNodes.OrderBy(kv => kv.Key).FirstOrDefault();
        return first.Value?.Position;
    }
}
