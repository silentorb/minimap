using Godot;
using Minimap.Automation;
using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Client;

/// <summary>World visuals + keyboard capture. Does not own or tick the simulation.</summary>
public partial class WorldView : Node2D, IMovementKeyTarget
{
    /// <summary>Scale for 16×16 Kenney tiles so pawns stay readable vs hex cells (~20px ColorRect).</summary>
    private const float SpriteFramesScale = 1.25f;

    [Export] public float HexSize { get; set; } = HexLayout.DefaultHexSize;

    private GameWorld? _world;
    private Random _rng = new(1);
    private Node2D? _hexLayer;
    private Node2D? _spawnerLayer;
    private Node2D? _placedLayer;
    private Node2D? _playerLayer;
    private Node2D? _missileLayer;
    private Node2D? _overlayLayer;
    private PackedScene? _hexScene;
    private PackedScene? _spawnerScene;
    private PackedScene? _playerScene;
    private readonly Dictionary<HexAxial, Node2D> _hexNodes = new();
    private readonly Dictionary<int, Node2D> _spawnerNodes = new();
    private readonly Dictionary<int, Node2D> _placedNodes = new();
    private readonly Dictionary<int, Node2D> _characterNodes = new();
    private readonly Dictionary<int, Node2D> _missileNodes = new();
    private readonly Dictionary<int, Line2D> _aimLines = new();
    private readonly HashSet<Key> _heldKeys = new();
    private readonly List<Character> _humanPawns = new();
    private HexAxial? _previewCell;
    private bool _previewValid;

    /// <summary>Raised after terrain visuals sync (evolution or level regen).</summary>
    public event Action? TerrainChanged;

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

        _placedLayer = GetNodeOrNull<Node2D>("PlacedLayer");
        if (_placedLayer is null)
        {
            _placedLayer = new Node2D { Name = "PlacedLayer" };
            AddChild(_placedLayer);
        }

        _playerLayer = GetNode<Node2D>("PlayerLayer");
        _missileLayer = GetNodeOrNull<Node2D>("MissileLayer");
        if (_missileLayer is null)
        {
            _missileLayer = new Node2D { Name = "MissileLayer" };
            AddChild(_missileLayer);
        }

        _overlayLayer = GetNodeOrNull<Node2D>("OverlayLayer");
        if (_overlayLayer is null)
        {
            _overlayLayer = new Node2D { Name = "OverlayLayer" };
            AddChild(_overlayLayer);
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

    public void SyncFrame(IReadOnlyList<PlayerController>? localPlayers = null)
    {
        SyncCharacters();
        SyncMissiles();
        SyncSpawners();
        SyncPlacedObjects();
        SyncPlacementPreview(localPlayers);
        SyncAimLines(localPlayers);
    }

    public void OnLevelRegenerated(IReadOnlyList<Character> humanPawns)
    {
        _humanPawns.Clear();
        _humanPawns.AddRange(humanPawns);
        SyncAll();
        RecenterCamera();
        TerrainChanged?.Invoke();
    }

    public SimVec2 ReadMoveInput() => ReadAxisFromKeys(Key.D, Key.A, Key.S, Key.W);

    public SimVec2 ReadMouseAimFrom(SimVec2 origin)
    {
        var mouse = GetGlobalMousePosition();
        var dir = new SimVec2(mouse.X - origin.X, mouse.Y - origin.Y);
        if (dir.LengthSquared < 1e-6f)
            return SimVec2.Zero;
        return dir.Normalized();
    }

    private void OnEvolutionTick()
    {
        if (_world is null)
            return;
        WorldEvolution.Tick(_world, _rng);
        SyncHexes();
        SyncCharacters();
        TerrainChanged?.Invoke();
    }

    private void SyncAll()
    {
        SyncHexes();
        SyncSpawners();
        SyncPlacedObjects();
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
            poly.Color = ColorForCell(h);
        }
    }

    private Color ColorForCell(HexAxial h)
    {
        if (_world is null)
            return new Color(0.1f, 0.1f, 0.12f);

        var baseColor = ColorFor(_world.Grid.Get(h));
        if (_previewCell is HexAxial preview && preview.Equals(h))
        {
            return _previewValid
                ? baseColor.Lerp(new Color(0.45f, 0.85f, 0.55f), 0.55f)
                : baseColor.Lerp(new Color(0.9f, 0.25f, 0.3f), 0.65f);
        }

        return baseColor;
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

    private void SyncPlacedObjects()
    {
        if (_world is null || _placedLayer is null)
            return;

        var live = new HashSet<int>();
        foreach (var placed in _world.PlacedObjects.Values)
        {
            live.Add(placed.Id);
            if (!_placedNodes.TryGetValue(placed.Id, out var node))
            {
                node = CreatePlacedObjectNode(placed);
                _placedLayer.AddChild(node);
                _placedNodes[placed.Id] = node;
            }

            var worldPos = HexLayout.ToWorld(placed.Cell, HexSize);
            node.Position = new Vector2(worldPos.X, worldPos.Y);
            node.ZIndex = 1;
        }

        foreach (var id in _placedNodes.Keys.ToList())
        {
            if (live.Contains(id))
                continue;
            _placedNodes[id].QueueFree();
            _placedNodes.Remove(id);
        }
    }

    private static Node2D CreatePlacedObjectNode(PlacedObject placed)
    {
        var node = new Node2D();
        var depiction = placed.Definition.DepictionConfig;
        if (depiction is not null &&
            depiction.Kind == DepictionKinds.Texture &&
            TryApplyTexture(node, depiction))
        {
            return node;
        }

        var rect = new ColorRect
        {
            Color = new Color(0.55f, 0.75f, 0.35f),
            OffsetLeft = -8,
            OffsetTop = -8,
            OffsetRight = 8,
            OffsetBottom = 8,
        };
        node.AddChild(rect);
        return node;
    }

    private static bool TryApplyTexture(Node2D node, DepictionConfig depiction)
    {
        var texture = GD.Load<Texture2D>(depiction.ResourcePath);
        if (texture is null)
            return false;

        var sprite = new Sprite2D
        {
            Texture = texture,
            Scale = new Vector2(0.045f, 0.045f),
        };
        node.AddChild(sprite);
        return true;
    }

    private void SyncPlacementPreview(IReadOnlyList<PlayerController>? localPlayers)
    {
        HexAxial? preview = null;
        var valid = false;
        if (localPlayers is not null)
        {
            foreach (var player in localPlayers)
            {
                if (!player.IsPlacementPreviewing || player.PlacementPreviewCell is not HexAxial cell)
                    continue;
                preview = cell;
                valid = player.PlacementPreviewValid;
                break;
            }
        }

        var changed = !_previewCell.Equals(preview) || _previewValid != valid;
        _previewCell = preview;
        _previewValid = valid;
        if (changed)
            SyncHexes();
    }

    private void SyncAimLines(IReadOnlyList<PlayerController>? localPlayers)
    {
        if (_overlayLayer is null)
            return;

        var live = new HashSet<int>();
        if (localPlayers is not null)
        {
            foreach (var player in localPlayers)
            {
                var pawn = player.Pawn;
                if (pawn is null || !pawn.IsAlive)
                    continue;

                live.Add(pawn.Id);
                if (!_aimLines.TryGetValue(pawn.Id, out var line))
                {
                    line = new Line2D
                    {
                        Width = 2f,
                        DefaultColor = new Color(1f, 1f, 1f, 0.65f),
                        Antialiased = true,
                    };
                    _overlayLayer.AddChild(line);
                    _aimLines[pawn.Id] = line;
                }

                var facing = pawn.Facing;
                if (facing.LengthSquared < 1e-10f)
                    facing = new SimVec2(1f, 0f);
                facing = facing.Normalized();
                const float length = 18f;
                line.Points =
                [
                    Vector2.Zero,
                    new Vector2(facing.X * length, facing.Y * length),
                ];
                line.Position = new Vector2(pawn.Position.X, pawn.Position.Y);
                line.ZIndex = 4;
            }
        }

        foreach (var id in _aimLines.Keys.ToList())
        {
            if (live.Contains(id))
                continue;
            _aimLines[id].QueueFree();
            _aimLines.Remove(id);
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
                ApplyCharacterDepiction(node, character);
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

    private static void ApplyCharacterDepiction(Node2D node, Character character)
    {
        var colorRect = node.GetNode<ColorRect>("ColorRect");
        var sprite = node.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        var depiction = character.Definition.DepictionConfig;

        if (sprite is not null &&
            depiction is not null &&
            depiction.Kind == DepictionKinds.SpriteFrames &&
            TryApplySpriteFrames(sprite, depiction))
        {
            colorRect.Visible = false;
            sprite.Visible = true;
            sprite.Modulate = FactionColor(character.FactionId, character.Id);
            return;
        }

        colorRect.Visible = true;
        colorRect.Color = FactionColor(character.FactionId, character.Id);
        if (sprite is not null)
            sprite.Visible = false;
    }

    private static bool TryApplySpriteFrames(AnimatedSprite2D sprite, DepictionConfig depiction)
    {
        var frames = GD.Load<SpriteFrames>(depiction.ResourcePath);
        if (frames is null)
            return false;

        sprite.SpriteFrames = frames;
        sprite.Scale = new Vector2(SpriteFramesScale, SpriteFramesScale);

        var animation = depiction.DefaultAnimation;
        if (string.IsNullOrWhiteSpace(animation) || !frames.HasAnimation(animation))
            animation = "default";

        if (!frames.HasAnimation(animation))
            return false;

        sprite.Play(animation);
        return true;
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

    private SimVec2 ReadAxisFromKeys(Key right, Key left, Key down, Key up)
    {
        var x = 0f;
        var y = 0f;
        if (_heldKeys.Contains(right) || Input.IsKeyPressed(right))
            x += 1f;
        if (_heldKeys.Contains(left) || Input.IsKeyPressed(left))
            x -= 1f;
        if (_heldKeys.Contains(down) || Input.IsKeyPressed(down))
            y += 1f;
        if (_heldKeys.Contains(up) || Input.IsKeyPressed(up))
            y -= 1f;
        return new SimVec2(x, y);
    }

    private static bool IsMovementKey(Key key) =>
        key is Key.W or Key.A or Key.S or Key.D;

    private static Color ColorFor(CellType t) =>
        t switch
        {
            CellType.Grass => new Color(0.35f, 0.42f, 0.38f),
            CellType.Wall => new Color(0.18f, 0.16f, 0.22f),
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
        if (playerIndex < 0 || playerIndex >= _humanPawns.Count)
            return null;

        var pawn = _humanPawns[playerIndex];
        if (_world is null || !_world.Characters.Contains(pawn))
            return null;

        return new Vector2(pawn.Position.X, pawn.Position.Y);
    }
}
