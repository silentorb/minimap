namespace Minimap.Simulation;

/// <summary>Authoritative simulation: terrain, characters, controllers, missiles.</summary>
public sealed class GameWorld
{
    private readonly List<Character> _characters = new();
    private readonly List<IController> _controllers = new();
    private readonly List<Missile> _missiles = new();
    private readonly List<SimVec2[]> _wallPolygons = new();
    private readonly Random _random;
    private int _nextCharacterId;
    private int _nextMissileId;
    private PlayerController? _playerController;

    public static GameWorld Create(
        int radiusX,
        int radiusY,
        int seed,
        IWorldGenerator? generator = null,
        float hexSize = HexWorldLayout.DefaultHexSize,
        SpawnConfig? spawn = null)
    {
        var grid = new HexGrid(radiusX, radiusY, hexSize);
        var gen = generator ?? new SeededWorldGenerator();
        var rng = new Random(seed);
        gen.GenerateTerrain(grid, rng);
        var world = new GameWorld(grid, hexSize, rng);
        world.SpawnDefaultRoster(spawn ?? new SpawnConfig(), hexSize);
        return world;
    }

    /// <summary>Equal-axis convenience (tests).</summary>
    public static GameWorld Create(
        int radius,
        int seed,
        IWorldGenerator? generator = null,
        float hexSize = HexWorldLayout.DefaultHexSize,
        SpawnConfig? spawn = null) =>
        Create(radius, radius, seed, generator, hexSize, spawn);

    public GameWorld(HexGrid grid, float hexSize, Random? random = null)
    {
        Grid = grid;
        HexSize = hexSize;
        MoveSpeed = CombatTuning.MoveSpeed;
        PlayerRadius = hexSize * 0.35f;
        MissileRadius = PlayerRadius * 0.45f;
        _random = random ?? new Random(1);
        RebuildWallColliders();
    }

    public HexGrid Grid { get; }
    public float HexSize { get; }
    public float MoveSpeed { get; set; }
    public float PlayerRadius { get; set; }
    public float MissileRadius { get; set; }
    public int TickIndex { get; private set; }

    public IReadOnlyList<Character> Characters => _characters;
    public IReadOnlyList<Missile> Missiles => _missiles;
    public IReadOnlyList<IController> Controllers => _controllers;
    public PlayerController? PlayerController => _playerController;

    /// <summary>Solid hex polygons (walls + out-of-map boundary cells).</summary>
    public IReadOnlyList<SimVec2[]> WallPolygons => _wallPolygons;

    public void AdvanceTick() => TickIndex++;

    public Character AddCharacter(int factionId, SimVec2 position, float maxHealth = CombatTuning.DefaultMaxHealth)
    {
        var c = new Character(_nextCharacterId++, factionId, position, maxHealth);
        _characters.Add(c);
        return c;
    }

    public void AttachController(IController controller, Character character)
    {
        controller.Possess(character);
        _controllers.Add(controller);
        if (controller is PlayerController pc)
            _playerController = pc;
    }

    public Missile SpawnMissile(
        SimVec2 position,
        SimVec2 velocity,
        float damage,
        int ownerFactionId,
        int? ownerCharacterId)
    {
        var m = new Missile(
            _nextMissileId++,
            position,
            velocity,
            MissileRadius,
            damage,
            ownerFactionId,
            ownerCharacterId);
        _missiles.Add(m);
        return m;
    }

    /// <summary>Full simulation step: controllers → movement → missiles → death prune.</summary>
    public void Tick(float dt)
    {
        if (dt <= 0f)
            return;

        foreach (var c in _controllers)
            c.Tick(this, dt);

        ApplyMovement(dt);
        TickMissiles(dt);
        RemoveDeadCharacters();
    }

    /// <summary>Movement only (tests that drive intents without controllers).</summary>
    public void TickMovement(float dt)
    {
        if (dt <= 0f)
            return;
        ApplyMovement(dt);
    }

    public void ApplyDamage(Character target, float amount)
    {
        if (!target.IsAlive || amount <= 0f)
            return;
        target.Health = MathF.Max(0f, target.Health - amount);
    }

    private void SpawnDefaultRoster(SpawnConfig spawn, float hexSize)
    {
        var total = 1 + spawn.AiPerFaction * 2;
        var hexes = SeededWorldGenerator.PickFloorSpawns(Grid, total, _random);
        var i = 0;

        var player = AddCharacter(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[i++], hexSize));
        AttachController(new PlayerController(), player);

        for (var a = 0; a < spawn.AiPerFaction; a++)
        {
            var ally = AddCharacter(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[i++], hexSize));
            AttachController(new AiController(_random), ally);
        }

        for (var a = 0; a < spawn.AiPerFaction; a++)
        {
            var rival = AddCharacter(spawn.RivalFactionId, HexWorldLayout.ToWorld(hexes[i++], hexSize));
            AttachController(new AiController(_random), rival);
        }
    }

    private void ApplyMovement(float dt)
    {
        foreach (var character in _characters)
        {
            if (!character.IsAlive)
                continue;
            var input = character.MoveIntent;
            if (input.LengthSquared < 1e-10f)
                continue;

            var dir = input.Normalized();
            var displacement = dir * (MoveSpeed * dt);
            character.Position = CircleHexCollision.MoveAndSlide(
                character.Position,
                displacement,
                PlayerRadius,
                _wallPolygons);
        }
    }

    private void TickMissiles(float dt)
    {
        for (var i = _missiles.Count - 1; i >= 0; i--)
        {
            var m = _missiles[i];
            m.Position += m.Velocity * dt;

            if (HitsWall(m))
            {
                _missiles.RemoveAt(i);
                continue;
            }

            var hit = false;
            foreach (var character in _characters)
            {
                if (!character.IsAlive)
                    continue;
                if (m.OwnerCharacterId is int oid && oid == character.Id)
                    continue;
                if (!FactionRules.AreHostile(m.OwnerFactionId, character.FactionId))
                    continue;

                var delta = character.Position - m.Position;
                var hitR = PlayerRadius + m.Radius;
                if (delta.LengthSquared <= hitR * hitR)
                {
                    ApplyDamage(character, m.Damage);
                    hit = true;
                    break;
                }
            }

            if (hit)
                _missiles.RemoveAt(i);
        }
    }

    private bool HitsWall(Missile m)
    {
        foreach (var wall in _wallPolygons)
        {
            if (CircleHexCollision.TryCircleConvex(m.Position, m.Radius, wall, out _, out var pen) && pen > 0f)
                return true;
        }

        return false;
    }

    private void RemoveDeadCharacters()
    {
        for (var i = _characters.Count - 1; i >= 0; i--)
        {
            if (_characters[i].IsAlive)
                continue;
            var dead = _characters[i];
            for (var c = _controllers.Count - 1; c >= 0; c--)
            {
                if (_controllers[c].Pawn?.Id != dead.Id)
                    continue;
                if (_controllers[c] is PlayerController)
                    _playerController = null;
                _controllers[c].Unpossess();
                _controllers.RemoveAt(c);
            }

            _characters.RemoveAt(i);
        }
    }

    /// <summary>Rebuild solid hex colliders from current terrain (call after evolution).</summary>
    public void RebuildWallColliders()
    {
        _wallPolygons.Clear();
        var seen = new HashSet<HexAxial>();

        foreach (var h in Grid.AllHexes())
        {
            if (Grid.Get(h) is CellType.Wall)
                AddWallHex(h, seen);

            foreach (var n in h.Neighbors())
            {
                if (!Grid.Contains(n))
                    AddWallHex(n, seen);
            }
        }
    }

    private void AddWallHex(HexAxial h, HashSet<HexAxial> seen)
    {
        if (!seen.Add(h))
            return;
        _wallPolygons.Add(HexWorldLayout.AbsoluteHexVertices(h, HexSize));
    }
}
