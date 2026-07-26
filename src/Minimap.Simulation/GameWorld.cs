using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Authoritative simulation: terrain, characters, controllers, missiles.</summary>
public sealed class GameWorld
{
    private readonly List<Character> _characters = new();
    private readonly List<IController> _controllers = new();
    private readonly List<Missile> _missiles = new();
    private readonly List<SwingArc> _swingArcs = new();
    private readonly List<Spawner> _spawners = new();
    private readonly Dictionary<HexAxial, Actor> _actorsByCell = new();
    private readonly Dictionary<string, ActorDefinition> _actorDefinitions =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, CharacterDefinition> _characterDefinitions =
        new(StringComparer.Ordinal);
    private readonly List<SimVec2[]> _wallPolygons = new();
    private readonly List<SimVec2> _characterObstacleCenters = new();
    private readonly Random _random;
    private CharacterDefinition? _spawnCharacterDefinition;
    private ResourceContext? _resourceContext;
    private WeightedPool<SpawnerDefinition> _worldSpawnerPool = WeightedPool<SpawnerDefinition>.Empty;
    private int _rivalFactionId = 2;
    private int _nextCharacterId;
    private int _nextMissileId;
    private int _nextSwingArcId;
    private int _nextSpawnerId;
    private int _nextActorId;


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
    public IReadOnlyList<Character> Characters => _characters;
    public IReadOnlyList<Missile> Missiles => _missiles;
    public IReadOnlyList<SwingArc> SwingArcs => _swingArcs;
    public IReadOnlyList<IController> Controllers => _controllers;
    public IReadOnlyList<Spawner> Spawners => _spawners;
    public IReadOnlyDictionary<HexAxial, Actor> CellActors => _actorsByCell;

    /// <summary>Shared RNG for content effects (spawn pools, nearby hex picks).</summary>
    public Random Random => _random;

    /// <summary>Definition used for spawns when callers omit an explicit definition.</summary>
    public CharacterDefinition? SpawnCharacterDefinition => _spawnCharacterDefinition;

    /// <summary>Resource types / health tags required before adding characters.</summary>
    public ResourceContext? ResourceContext => _resourceContext;

    /// <summary>Rival / zombie faction id used when emerging ambush characters.</summary>
    public int RivalFactionId
    {
        get => _rivalFactionId;
        set => _rivalFactionId = value;
    }

    /// <summary>Solid hex polygons (walls + out-of-map boundary cells).</summary>
    public IReadOnlyList<SimVec2[]> WallPolygons => _wallPolygons;

    public void SetSpawnCharacterDefinition(CharacterDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _spawnCharacterDefinition = definition;
    }

    public void SetResourceContext(ResourceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _resourceContext = context;
    }

    public void ApplyGameContent(GameContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        SetSpawnCharacterDefinition(content.DefaultCharacter);
        SetActorDefinitions(content.Actors);
        SetCharacterDefinitions(content.Characters);
        SetResourceContext(ResourceContext.FromGameContent(content));
    }

    public void SetCharacterDefinitions(IEnumerable<CharacterDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _characterDefinitions.Clear();
        foreach (var def in definitions)
        {
            ArgumentNullException.ThrowIfNull(def);
            if (!_characterDefinitions.TryAdd(def.Id, def))
            {
                throw new InvalidOperationException(
                    $"Duplicate character definition id '{def.Id}'.");
            }
        }
    }

    public bool TryGetCharacterDefinition(string id, out CharacterDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _characterDefinitions.TryGetValue(id, out definition);
    }

    public void SetActorDefinitions(IEnumerable<ActorDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _actorDefinitions.Clear();
        foreach (var def in definitions)
        {
            ArgumentNullException.ThrowIfNull(def);
            if (!_actorDefinitions.TryAdd(def.Id, def))
            {
                throw new InvalidOperationException(
                    $"Duplicate actor definition id '{def.Id}'.");
            }
        }
    }

    public bool TryGetActorDefinition(string id, out ActorDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            definition = null;
            return false;
        }

        return _actorDefinitions.TryGetValue(id, out definition);
    }

    public bool IsCellOccupied(HexAxial cell) => _actorsByCell.ContainsKey(cell);

    public bool TryGetActorAt(HexAxial cell, out Actor? actor) =>
        _actorsByCell.TryGetValue(cell, out actor);

    /// <summary>
    /// Places a cell-anchored actor on <paramref name="cell"/> if the cell is in the grid and empty.
    /// Returns false for expected rejection (missing cell / occupied); does not validate terrain.
    /// </summary>
    public bool TryPlaceActor(HexAxial cell, ActorDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!Grid.Contains(cell) || _actorsByCell.ContainsKey(cell))
            return false;

        var resources = _resourceContext
            ?? throw new InvalidOperationException(
                "Resource context is required (call SetResourceContext or ApplyGameContent).");
        var actor = new Actor(_nextActorId++, definition, resources);
        actor.Cell = cell;
        _actorsByCell[cell] = actor;
        return true;
    }

    public bool TryRemoveActorAt(HexAxial cell, out Actor? removed)
    {
        if (_actorsByCell.Remove(cell, out var obj))
        {
            obj.Cell = null;
            removed = obj;
            return true;
        }

        removed = null;
        return false;
    }

    /// <summary>Ticks passive effects on cell-anchored actors (e.g. grow / spawn).</summary>
    public void TickCellActors(float dt)
    {
        // Snapshot: grow emerge / spawn may mutate actors during the tick.
        var actors = _actorsByCell.Values.ToList();
        foreach (var actor in actors)
        {
            foreach (var effect in actor.Effects.ToList())
            {
                if (effect is IGrowEffect grow)
                    grow.Tick(this, actor, dt);
                else if (effect is ISpawnEffect spawn)
                    spawn.Tick(this, actor, dt);
                else if (effect is IPassiveEffect passive)
                    passive.Tick(actor, dt);
            }
        }
    }

    /// <summary>Spawn a high-aggression AI character (e.g. crazed carrot emerge).</summary>
    public Character SpawnChaseCharacter(
        CharacterDefinition definition,
        SimVec2 position,
        int factionId)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var character = AddCharacter(factionId, position, definition);
        AttachController(
            new AiController(
                _random,
                aggression: AiTuning.CrazedCarrotAggression,
                seekCrops: AiController.CharacterSeeksCrops(definition)),
            character);
        return character;
    }

    /// <summary>Ticks passive effects on characters (e.g. energy drain / vitality).</summary>
    public void TickCharacterPassives(float dt)
    {
        foreach (var character in _characters)
        {
            if (!character.IsAlive)
                continue;
            foreach (var effect in character.Effects)
            {
                if (effect is IPassiveEffect passive)
                    passive.Tick(character, dt);
            }
        }
    }

    public Character AddCharacter(
        int factionId,
        SimVec2 position,
        CharacterDefinition? definition = null,
        int maxHealth = CombatTuning.DefaultMaxHealth,
        int maxEnergy = CombatTuning.DefaultMaxEnergy)
    {
        var def = definition ?? _spawnCharacterDefinition
            ?? throw new InvalidOperationException(
                "Character definition is required (pass definition or call SetSpawnCharacterDefinition).");
        var resources = _resourceContext
            ?? throw new InvalidOperationException(
                "Resource context is required (call SetResourceContext or ApplyGameContent).");
        var c = new Character(_nextCharacterId++, factionId, position, def, resources, maxHealth, maxEnergy);
        _characters.Add(c);
        return c;
    }

    public void AttachController(IController controller, Character character)
    {
        controller.Possess(character);
        _controllers.Add(controller);
    }

    public SwingArc SpawnSwingArc(
        SimVec2 origin,
        SimVec2 facing,
        float radius,
        float arcDegrees,
        int damage,
        int ownerFactionId,
        int? ownerCharacterId,
        bool friendlyFire,
        float lifetimeSeconds)
    {
        var arc = new SwingArc(
            _nextSwingArcId++,
            origin,
            facing,
            radius,
            arcDegrees,
            damage,
            ownerFactionId,
            ownerCharacterId,
            friendlyFire,
            lifetimeSeconds);
        _swingArcs.Add(arc);
        ResolveSwingHits(arc);
        return arc;
    }

    public Missile SpawnMissile(
        SimVec2 position,
        SimVec2 velocity,
        int damage,
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
        GameContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(spawn);
        ApplyGameContent(content);
        RivalFactionId = spawn.RivalFactionId;
        SpawnHumanPlayers(spawn);
        PlaceSpawners(scenario.SpawnerCount, content.WorldSpawnerPool);
    }

    public void RegenerateLevel(
        Scenario scenario,
        SpawnConfig spawn,
        int levelIndex,
        WeightedPool<SpawnerDefinition>? spawnerPool = null)
    {
        ClearRivalsAndMissiles(spawn.RivalFactionId);
        _spawners.Clear();
        _actorsByCell.Clear();

        var rng = new Random(WorldSeed + levelIndex);
        var gen = new SeededWorldGenerator();
        gen.GenerateTerrain(Grid, rng);
        RebuildWallColliders();

        RepositionAndHealHumans(spawn);
        PlaceSpawners(scenario.SpawnerCount, spawnerPool ?? _worldSpawnerPool);
    }

    public void SpawnHumanPlayers(SpawnConfig spawn)
    {
        var humans = Math.Max(0, spawn.HumanPlayerCount);
        if (humans == 0)
            return;

        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, humans, _random);
        for (var h = 0; h < humans; h++)
            AddCharacter(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[h], HexSize));
    }

    /// <summary>
    /// Place <paramref name="count"/> marker spawners by weighted-picking from <paramref name="pool"/>.
    /// Empty pool places nothing. Kept for parked <see cref="ScenarioRunner"/> wave tests.
    /// </summary>
    public void PlaceSpawners(int count, WeightedPool<SpawnerDefinition> pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        _worldSpawnerPool = pool;
        _spawners.Clear();
        if (count <= 0 || pool.IsEmpty)
            return;

        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, count, _random);
        for (var i = 0; i < count; i++)
        {
            if (!pool.TryPick(_random, out var definition) || definition is null)
                break;

            _spawners.Add(new Spawner(_nextSpawnerId++, hexes[i], definition.CharacterPool));
        }
    }

    /// <summary>
    /// Place <paramref name="count"/> destructible intrinsic spawner actors (placeables) on grass.
    /// Missing definition places nothing.
    /// </summary>
    public void PlaceSpawnerActors(int count, string actorDefinitionId)
    {
        if (count <= 0 || string.IsNullOrWhiteSpace(actorDefinitionId))
            return;
        if (!TryGetActorDefinition(actorDefinitionId, out var definition) || definition is null)
            return;

        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, count, _random);
        for (var i = 0; i < count; i++)
            TryPlaceActor(hexes[i], definition);
    }

    public void SpawnWaveEnemies(Spawner spawner, int count, int rivalFactionId)
    {
        ArgumentNullException.ThrowIfNull(spawner);
        if (count <= 0 || spawner.CharacterPool.IsEmpty)
            return;

        for (var i = 0; i < count; i++)
        {
            if (!spawner.CharacterPool.TryPick(_random, out var definition) || definition is null)
                break;

            TrySpawnNearbyHostile(
                spawner.Position,
                definition,
                rivalFactionId,
                AiTuning.DefaultAggression,
                AiController.CharacterSeeksCrops(definition));
        }
    }

    /// <summary>
    /// Spawn one rival AI on a nearby grass hex. Returns null when no grass candidates exist.
    /// </summary>
    public Character? TrySpawnNearbyHostile(
        HexAxial origin,
        CharacterDefinition definition,
        int rivalFactionId,
        float aggression = AiTuning.DefaultAggression,
        bool seekCrops = false)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var candidates = CollectNearbyGrassHexes(origin, maxDistance: 2);
        if (candidates.Count == 0)
        {
            candidates = Grid.AllHexes()
                .Where(h => Grid.Get(h) == CellType.Grass)
                .ToList();
        }

        if (candidates.Count == 0)
            return null;

        var hex = candidates[_random.Next(candidates.Count)];
        var enemy = AddCharacter(
            rivalFactionId,
            HexWorldLayout.ToWorld(hex, HexSize),
            definition);
        AttachController(new AiController(_random, aggression: aggression, seekCrops: seekCrops), enemy);
        return enemy;
    }

    /// <summary>Legacy bootstrap roster (humans + ally AI + rival AI). Kept for tests.</summary>
    public void SpawnDefaultRoster(
        SpawnConfig spawn,
        CharacterDefinition characterDefinition,
        ResourceContext? resourceContext = null)
    {
        SetSpawnCharacterDefinition(characterDefinition);
        if (resourceContext is not null)
            SetResourceContext(resourceContext);
        var humans = Math.Max(0, spawn.HumanPlayerCount);
        var total = humans + spawn.AiPerFaction * 2;
        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, total, _random);
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

    /// <summary>Full simulation step: cell actors → controllers → movement → missiles → death prune.</summary>
    public void Tick(float dt)
    {
        if (dt <= 0f)
            return;

        TickCellActors(dt);
        TickCharacterPassives(dt);

        foreach (var c in _controllers)
            c.Tick(this, dt);

        ApplyMovement(dt);
        TickMissiles(dt);
        TickSwingArcs(dt);
        RemoveDeadActors();
    }

    /// <summary>Movement only (tests that drive intents without controllers).</summary>
    public void TickMovement(float dt)
    {
        if (dt <= 0f)
            return;
        ApplyMovement(dt);
    }

    public void ApplyDamage(Actor target, int amount)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!target.IsDestructible || !target.IsAlive || amount <= 0)
            return;
        target.Health = Math.Max(0, target.Health - amount);
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
        _swingArcs.Clear();

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

        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, humans.Count, _random);
        for (var i = 0; i < humans.Count; i++)
        {
            humans[i].Position = HexWorldLayout.ToWorld(hexes[i], HexSize);
            humans[i].Health = humans[i].MaxHealth;
        }
    }

    private List<HexAxial> CollectNearbyGrassHexes(HexAxial center, int maxDistance)
    {
        var result = new List<HexAxial>();
        foreach (var h in Grid.AllHexes())
        {
            if (Grid.Get(h) != CellType.Grass)
                continue;

            var distance = HexAxial.Distance(center, h);
            if (distance >= 1 && distance <= maxDistance)
                result.Add(h);
        }

        if (result.Count > 0)
            return result;

        foreach (var h in Grid.AllHexes())
        {
            if (Grid.Get(h) == CellType.Grass && HexAxial.Distance(center, h) <= maxDistance)
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
            {
                var dir = input.Normalized();
                character.Facing = dir;
                displacement = dir * (MoveSpeed * dt);
            }

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

            if (TryMissileHitCharacter(m) || TryMissileHitCellActor(m))
                _missiles.RemoveAt(i);
        }
    }

    private bool TryMissileHitCharacter(Missile m)
    {
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
                return true;
            }
        }

        return false;
    }

    private bool TryMissileHitCellActor(Missile m)
    {
        foreach (var (cell, actor) in _actorsByCell)
        {
            if (!actor.IsDestructible || !actor.IsAlive)
                continue;

            var center = HexWorldLayout.ToWorld(cell, HexSize);
            var delta = center - m.Position;
            var hitR = PlayerRadius + m.Radius;
            if (delta.LengthSquared <= hitR * hitR)
            {
                ApplyDamage(actor, m.Damage);
                return true;
            }
        }

        return false;
    }

    private void TickSwingArcs(float dt)
    {
        for (var i = _swingArcs.Count - 1; i >= 0; i--)
        {
            _swingArcs[i].TimeRemaining -= dt;
            if (_swingArcs[i].TimeRemaining <= 0f)
                _swingArcs.RemoveAt(i);
        }
    }

    private void ResolveSwingHits(SwingArc arc)
    {
        foreach (var character in _characters)
        {
            if (!character.IsAlive)
                continue;
            if (arc.OwnerCharacterId is int oid && oid == character.Id)
                continue;
            if (!arc.FriendlyFire && !FactionRules.AreHostile(arc.OwnerFactionId, character.FactionId))
                continue;

            if (Swing.IsPointInArc(
                    arc.Origin,
                    arc.Facing,
                    arc.Radius,
                    arc.ArcDegrees,
                    character.Position,
                    PlayerRadius))
            {
                ApplyDamage(character, arc.Damage);
            }
        }

        foreach (var (cell, actor) in _actorsByCell)
        {
            if (!actor.IsDestructible || !actor.IsAlive)
                continue;

            var center = HexWorldLayout.ToWorld(cell, HexSize);
            if (Swing.IsPointInArc(
                    arc.Origin,
                    arc.Facing,
                    arc.Radius,
                    arc.ArcDegrees,
                    center,
                    PlayerRadius))
            {
                ApplyDamage(actor, arc.Damage);
            }
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

    private void RemoveDeadActors()
    {
        for (var i = _characters.Count - 1; i >= 0; i--)
        {
            if (_characters[i].IsAlive)
                continue;
            var dead = _characters[i];
            TryPlaceDeathDrops(dead);
            DetachControllersFor(dead);
            _characters.RemoveAt(i);
        }

        List<HexAxial>? deadCells = null;
        foreach (var (cell, actor) in _actorsByCell)
        {
            if (actor.IsAlive)
                continue;
            deadCells ??= new List<HexAxial>();
            deadCells.Add(cell);
        }

        if (deadCells is null)
            return;

        foreach (var cell in deadCells)
            TryRemoveActorAt(cell, out _);
    }

    private void TryPlaceDeathDrops(Character dead)
    {
        foreach (var effect in dead.Effects)
        {
            if (effect is not IDeathDropEffect drop)
                continue;
            if (!TryGetActorDefinition(drop.ActorDefinitionId, out var actorDef) || actorDef is null)
                continue;

            var cell = HexWorldLayout.WorldToAxial(dead.Position, HexSize);
            if (!Grid.Contains(cell) || IsCellOccupied(cell))
                continue;
            TryPlaceActor(cell, actorDef);
        }
    }

    /// <summary>Rebuild solid hex colliders from current terrain.</summary>
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
