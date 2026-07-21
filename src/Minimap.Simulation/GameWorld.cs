using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Authoritative simulation: terrain, characters, controllers, missiles.</summary>
public sealed class GameWorld
{
    private readonly List<Character> _characters = new();
    private readonly List<IController> _controllers = new();
    private readonly List<Missile> _missiles = new();
    private readonly List<WaveSpawner> _spawners = new();
    private readonly List<SimVec2[]> _wallPolygons = new();
    private readonly List<SimVec2> _characterObstacleCenters = new();
    private readonly Random _random;
    private CharacterDefinition? _spawnCharacterDefinition;
    private int _nextCharacterId;
    private int _nextMissileId;
    private int _nextSpawnerId;


    public static GameWorld Create(
        int radiusX,
        int radiusY,
        int seed,
        IWorldGenerator? generator = null,
        float hexSize = HexWorldLayout.DefaultHexSize)
    {
        var grid = new HexGrid(radiusX, radiusY, hexSize);
        var gen = generator ?? new SeededWorldGenerator();
        var rng = new Random(seed);
        gen.GenerateTerrain(grid, rng);
        return new GameWorld(grid, hexSize, rng, seed);
    }

    /// <summary>Equal-axis convenience (tests).</summary>
    public static GameWorld Create(
        int radius,
        int seed,
        IWorldGenerator? generator = null,
        float hexSize = HexWorldLayout.DefaultHexSize) =>
        Create(radius, radius, seed, generator, hexSize);

    public GameWorld(HexGrid grid, float hexSize, Random? random = null, int worldSeed = 0)
    {
        Grid = grid;
        HexSize = hexSize;
        WorldSeed = worldSeed;
        MoveSpeed = CombatTuning.MoveSpeed;
        PlayerRadius = hexSize * 0.35f;
        MissileRadius = PlayerRadius * 0.45f;
        _random = random ?? new Random(1);
        RebuildWallColliders();
    }

    public HexGrid Grid { get; }
    public float HexSize { get; }
    public int WorldSeed { get; }
    public float MoveSpeed { get; set; }
    public float PlayerRadius { get; set; }
    public float MissileRadius { get; set; }
    public int TickIndex { get; private set; }

    public IReadOnlyList<Character> Characters => _characters;
    public IReadOnlyList<Missile> Missiles => _missiles;
    public IReadOnlyList<IController> Controllers => _controllers;
    public IReadOnlyList<WaveSpawner> Spawners => _spawners;

    /// <summary>Definition used for spawns when callers omit an explicit definition.</summary>
    public CharacterDefinition? SpawnCharacterDefinition => _spawnCharacterDefinition;

    /// <summary>Solid hex polygons (walls + out-of-map boundary cells).</summary>
    public IReadOnlyList<SimVec2[]> WallPolygons => _wallPolygons;

    public void AdvanceTick() => TickIndex++;

    public void SetSpawnCharacterDefinition(CharacterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _spawnCharacterDefinition = definition;
    }

    public Character AddCharacter(
        int factionId,
        SimVec2 position,
        CharacterDefinition? definition = null,
        float maxHealth = CombatTuning.DefaultMaxHealth)
    {
        var def = definition ?? _spawnCharacterDefinition
            ?? throw new InvalidOperationException(
                "Character definition is required (pass definition or call SetSpawnCharacterDefinition).");
        var c = new Character(_nextCharacterId++, factionId, position, def, maxHealth);
        _characters.Add(c);
        return c;
    }

    public void AttachController(IController controller, Character character)
    {
        controller.Possess(character);
        _controllers.Add(controller);
    }

    public Missile SpawnMissile(
        SimVec2 position,
        SimVec2 velocity,
        float damage,
        int ownerFactionId,
        int? ownerCharacterId,
        bool friendlyFire = true)
    {
        var m = new Missile(
            _nextMissileId++,
            position,
            velocity,
            MissileRadius,
            damage,
            ownerFactionId,
            ownerCharacterId,
            friendlyFire);
        _missiles.Add(m);
        return m;
    }

    public void InitializeScenarioLevel(
        Scenario scenario,
        SpawnConfig spawn,
        CharacterDefinition characterDefinition)
    {
        SetSpawnCharacterDefinition(characterDefinition);
        SpawnHumanPlayers(spawn);
        PlaceWaveSpawners(scenario.SpawnerCount);
    }

    public void RegenerateLevel(Scenario scenario, SpawnConfig spawn, int levelIndex)
    {
        ClearRivalsAndMissiles(spawn.RivalFactionId);
        _spawners.Clear();

        var rng = new Random(WorldSeed + levelIndex);
        var gen = new SeededWorldGenerator();
        gen.GenerateTerrain(Grid, rng);
        RebuildWallColliders();

        RepositionAndHealHumans(spawn);
        PlaceWaveSpawners(scenario.SpawnerCount);
    }

    public void SpawnHumanPlayers(SpawnConfig spawn)
    {
        var humans = Math.Max(0, spawn.HumanPlayerCount);
        if (humans == 0)
            return;

        var hexes = SeededWorldGenerator.PickFloorSpawns(Grid, humans, _random);
        for (var h = 0; h < humans; h++)
            AddCharacter(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[h], HexSize));
    }

    public void PlaceWaveSpawners(int count)
    {
        _spawners.Clear();
        if (count <= 0)
            return;

        var hexes = SeededWorldGenerator.PickFloorSpawns(Grid, count, _random);
        for (var i = 0; i < count; i++)
            _spawners.Add(new WaveSpawner(_nextSpawnerId++, hexes[i]));
    }

    public void SpawnWaveEnemies(WaveSpawner spawner, int count, int rivalFactionId)
    {
        if (count <= 0)
            return;

        var candidates = CollectNearbyFloorHexes(spawner.Position, maxDistance: 2);
        if (candidates.Count == 0)
        {
            candidates = Grid.AllHexes()
                .Where(h => Grid.Get(h) == CellType.Floor)
                .ToList();
        }

        for (var i = 0; i < count; i++)
        {
            var hex = candidates[_random.Next(candidates.Count)];
            var enemy = AddCharacter(rivalFactionId, HexWorldLayout.ToWorld(hex, HexSize));
            AttachController(new AiController(_random), enemy);
        }
    }

    /// <summary>Legacy bootstrap roster (humans + ally AI + rival AI). Kept for tests.</summary>
    public void SpawnDefaultRoster(SpawnConfig spawn, CharacterDefinition characterDefinition)
    {
        SetSpawnCharacterDefinition(characterDefinition);
        var humans = Math.Max(0, spawn.HumanPlayerCount);
        var total = humans + spawn.AiPerFaction * 2;
        var hexes = SeededWorldGenerator.PickFloorSpawns(Grid, total, _random);
        var i = 0;

        for (var h = 0; h < humans; h++)
            AddCharacter(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[i++], HexSize));

        for (var a = 0; a < spawn.AiPerFaction; a++)
        {
            var ally = AddCharacter(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[i++], HexSize));
            AttachController(new AiController(_random), ally);
        }

        for (var a = 0; a < spawn.AiPerFaction; a++)
        {
            var rival = AddCharacter(spawn.RivalFactionId, HexWorldLayout.ToWorld(hexes[i++], HexSize));
            AttachController(new AiController(_random), rival);
        }
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

    /// <summary>Remove a living character and detach its controller (player drop).</summary>
    public void ForceRemoveCharacter(Character character)
    {
        DetachControllersFor(character);
        _characters.Remove(character);
    }

    private void ClearRivalsAndMissiles(int rivalFactionId)
    {
        _missiles.Clear();

        for (var i = _characters.Count - 1; i >= 0; i--)
        {
            if (_characters[i].FactionId != rivalFactionId)
                continue;

            DetachControllersFor(_characters[i]);
            _characters.RemoveAt(i);
        }
    }

    private void RepositionAndHealHumans(SpawnConfig spawn)
    {
        var playerFaction = spawn.PlayerFactionId;
        var humans = _characters.Where(c => c.FactionId == playerFaction).ToList();
        var needed = Math.Max(0, spawn.HumanPlayerCount);

        while (humans.Count < needed)
            humans.Add(AddCharacter(playerFaction, SimVec2.Zero));

        if (humans.Count == 0)
            return;

        var hexes = SeededWorldGenerator.PickFloorSpawns(Grid, humans.Count, _random);
        for (var i = 0; i < humans.Count; i++)
        {
            humans[i].Position = HexWorldLayout.ToWorld(hexes[i], HexSize);
            humans[i].Health = humans[i].MaxHealth;
        }
    }

    private List<HexAxial> CollectNearbyFloorHexes(HexAxial center, int maxDistance)
    {
        var result = new List<HexAxial>();
        foreach (var h in Grid.AllHexes())
        {
            if (Grid.Get(h) != CellType.Floor)
                continue;

            var distance = HexAxial.Distance(center, h);
            if (distance >= 1 && distance <= maxDistance)
                result.Add(h);
        }

        if (result.Count > 0)
            return result;

        foreach (var h in Grid.AllHexes())
        {
            if (Grid.Get(h) == CellType.Floor && HexAxial.Distance(center, h) <= maxDistance)
                result.Add(h);
        }

        return result;
    }

    private void DetachControllersFor(Character character)
    {
        for (var c = _controllers.Count - 1; c >= 0; c--)
        {
            if (_controllers[c].Pawn?.Id != character.Id)
                continue;
            _controllers[c].Unpossess();
            _controllers.RemoveAt(c);
        }
    }

    private void ApplyMovement(float dt)
    {
        foreach (var character in _characters)
        {
            if (!character.IsAlive)
                continue;

            var input = character.MoveIntent;
            var displacement = SimVec2.Zero;
            if (input.LengthSquared >= 1e-10f)
                displacement = input.Normalized() * (MoveSpeed * dt);

            CollectCharacterObstacles(character.Id);
            character.Position = CircleHexCollision.MoveAndSlide(
                character.Position,
                displacement,
                PlayerRadius,
                _wallPolygons,
                _characterObstacleCenters,
                PlayerRadius);
        }
    }

    private void CollectCharacterObstacles(int excludeCharacterId)
    {
        _characterObstacleCenters.Clear();
        foreach (var other in _characters)
        {
            if (!other.IsAlive || other.Id == excludeCharacterId)
                continue;
            _characterObstacleCenters.Add(other.Position);
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
                if (!m.FriendlyFire && !FactionRules.AreHostile(m.OwnerFactionId, character.FactionId))
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
            DetachControllersFor(dead);
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
