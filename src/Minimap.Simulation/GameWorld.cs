using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Authoritative simulation: terrain, actors, controllers, projectiles.</summary>
public sealed class GameWorld
{
    private readonly List<Actor> _actors = new();
    private readonly List<IController> _controllers = new();
    private readonly List<SwingArc> _swingArcs = new();
    private readonly List<Spawner> _spawners = new();
    private readonly Dictionary<HexAxial, Actor> _actorsByCell = new();
    private readonly Dictionary<string, ActorDefinition> _actorDefinitions =
        new(StringComparer.Ordinal);
    private readonly List<SimVec2[]> _wallPolygons = new();
    private readonly List<SimVec2> _actorObstacleCenters = new();
    private readonly Random _random;
    private ActorDefinition? _spawnActorDefinition;
    private ResourceContext? _resourceContext;
    private WeightedPool<SpawnerDefinition> _worldSpawnerPool = WeightedPool<SpawnerDefinition>.Empty;
    private int _rivalFactionId = 2;
    private int _nextActorId;
    private int _nextSwingArcId;
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
        _random = random ?? new Random(1);
        RebuildWallColliders();
    }

    public HexGrid Grid { get; }
    public float HexSize { get; }
    public int WorldSeed { get; }
    public float MoveSpeed { get; set; }
    public float PlayerRadius { get; set; }
    public IReadOnlyList<Actor> Actors => _actors;
    public IReadOnlyList<SwingArc> SwingArcs => _swingArcs;
    public IReadOnlyList<IController> Controllers => _controllers;
    public IReadOnlyList<Spawner> Spawners => _spawners;
    public IReadOnlyDictionary<HexAxial, Actor> CellActors => _actorsByCell;

    /// <summary>Shared RNG for content effects (spawn pools, nearby hex picks).</summary>
    public Random Random => _random;

    /// <summary>Definition used for spawns when callers omit an explicit definition.</summary>
    public ActorDefinition? SpawnActorDefinition => _spawnActorDefinition;

    /// <summary>Resource types / health tags required before adding actors.</summary>
    public ResourceContext? ResourceContext => _resourceContext;

    /// <summary>Rival / zombie faction id used when emerging ambush actors.</summary>
    public int RivalFactionId
    {
        get => _rivalFactionId;
        set => _rivalFactionId = value;
    }

    /// <summary>Solid hex polygons (walls + out-of-map boundary cells).</summary>
    public IReadOnlyList<SimVec2[]> WallPolygons => _wallPolygons;

    public void SetSpawnActorDefinition(ActorDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _spawnActorDefinition = definition;
    }

    public void SetResourceContext(ResourceContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _resourceContext = context;
    }

    public void ApplyGameContent(GameContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        SetSpawnActorDefinition(content.DefaultActor);
        SetActorDefinitions(content.Actors);
        SetResourceContext(ResourceContext.FromGameContent(content));
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
    public bool TryPlaceActor(HexAxial cell, ActorDefinition definition, int factionId = 0)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!Grid.Contains(cell) || _actorsByCell.ContainsKey(cell))
            return false;

        var resources = _resourceContext
            ?? throw new InvalidOperationException(
                "Resource context is required (call SetResourceContext or ApplyGameContent).");
        var actor = new Actor(
            _nextActorId++,
            definition,
            resources,
            factionId,
            HexWorldLayout.ToWorld(cell, HexSize));
        actor.Cell = cell;
        _actors.Add(actor);
        _actorsByCell[cell] = actor;
        return true;
    }

    public bool TryRemoveActorAt(HexAxial cell, out Actor? removed)
    {
        if (_actorsByCell.Remove(cell, out var obj))
        {
            obj.Cell = null;
            _actors.Remove(obj);
            removed = obj;
            return true;
        }

        removed = null;
        return false;
    }

    /// <summary>Ticks passive effects on all actors (grow / spawn / hunger / companions).</summary>
    public void TickActorPassives(float dt)
    {
        // Snapshot: grow emerge / spawn / companions may mutate actors during the tick.
        var actors = _actors.ToList();
        foreach (var actor in actors)
        {
            if (!actor.IsAlive)
                continue;
            foreach (var effect in actor.Effects.ToList())
            {
                if (effect is IGrowEffect grow)
                    grow.Tick(this, actor, dt);
                else if (effect is ISpawnEffect spawn)
                    spawn.Tick(this, actor, dt);
                else if (effect is IWorldActorPassiveEffect worldActorPassive)
                    worldActorPassive.Tick(this, actor, dt);
                else if (effect is IWorldPassiveEffect worldPassive)
                    worldPassive.Tick(this, actor, dt);
                else if (effect is IPassiveEffect passive)
                    passive.Tick(actor, dt);
            }
        }
    }

    /// <summary>Spawn a high-aggression AI actor (e.g. crazed carrot emerge).</summary>
    public Actor SpawnChaseActor(
        ActorDefinition definition,
        SimVec2 position,
        int factionId)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var actor = AddActor(factionId, position, definition);
        AttachController(
            new AiController(
                _random,
                aggression: AiTuning.CrazedCarrotAggression,
                seekCrops: AiController.ActorSeeksCrops(definition)),
            actor);
        return actor;
    }

    public Actor AddActor(
        int factionId,
        SimVec2 position,
        ActorDefinition? definition = null)
    {
        var def = definition ?? _spawnActorDefinition
            ?? throw new InvalidOperationException(
                "Actor definition is required (pass definition or call SetSpawnActorDefinition).");
        var resources = _resourceContext
            ?? throw new InvalidOperationException(
                "Resource context is required (call SetResourceContext or ApplyGameContent).");
        var actor = new Actor(_nextActorId++, def, resources, factionId, position);
        _actors.Add(actor);
        return actor;
    }

    public void AttachController(IController controller, Actor actor)
    {
        controller.Possess(actor);
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
        ClearCellAnchoredActors();

        var rng = new Random(WorldSeed + levelIndex);
        var gen = new SeededWorldGenerator();
        gen.GenerateTerrain(Grid, rng);
        RebuildWallColliders();

        RepositionAndHealHumans(spawn);
        if (spawnerPool is not null)
            _worldSpawnerPool = spawnerPool;
        // Live path: intrinsic placeables (wave-clock marker emission is separate / optional).
        PlaceSpawnerActors(scenario.SpawnerCount, AiTuning.ZombieSpawnerActorId);
    }

    public void SpawnHumanPlayers(SpawnConfig spawn)
    {
        var humans = Math.Max(0, spawn.HumanPlayerCount);
        if (humans == 0)
            return;

        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, humans, _random);
        for (var h = 0; h < humans; h++)
            AddActor(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[h], HexSize));
    }

    /// <summary>
    /// Place <paramref name="count"/> marker spawners by weighted-picking from <paramref name="pool"/>.
    /// Empty pool places nothing. Used by tests and future wave-burst events; normal play uses
    /// <see cref="PlaceSpawnerActors"/> instead.
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

            _spawners.Add(new Spawner(_nextSpawnerId++, hexes[i], definition.ActorPool));
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
        if (count <= 0 || spawner.ActorPool.IsEmpty)
            return;

        for (var i = 0; i < count; i++)
        {
            if (!spawner.ActorPool.TryPick(_random, out var definition) || definition is null)
                break;

            TrySpawnNearbyHostile(
                spawner.Position,
                definition,
                rivalFactionId,
                AiTuning.DefaultAggression,
                AiController.ActorSeeksCrops(definition));
        }
    }

    /// <summary>
    /// Spawn one rival AI on a nearby grass hex. Returns null when no grass candidates exist.
    /// </summary>
    public Actor? TrySpawnNearbyHostile(
        HexAxial origin,
        ActorDefinition definition,
        int rivalFactionId,
        float aggression = AiTuning.DefaultAggression,
        bool seekCrops = false) =>
        TrySpawnNearbyActor(origin, definition, rivalFactionId, aggression, seekCrops);

    /// <summary>
    /// Spawn one AI actor on a nearby grass hex. Returns null when no grass candidates exist.
    /// </summary>
    public Actor? TrySpawnNearbyActor(
        HexAxial origin,
        ActorDefinition definition,
        int factionId,
        float aggression = AiTuning.DefaultAggression,
        bool seekCrops = false,
        int? ownerActorId = null)
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
        var spawned = AddActor(
            factionId,
            HexWorldLayout.ToWorld(hex, HexSize),
            definition);
        spawned.OwnerActorId = ownerActorId;
        AttachController(new AiController(_random, aggression: aggression, seekCrops: seekCrops), spawned);
        return spawned;
    }

    /// <summary>Legacy bootstrap roster (humans + ally AI + rival AI). Kept for tests.</summary>
    public void SpawnDefaultRoster(
        SpawnConfig spawn,
        ActorDefinition actorDefinition,
        ResourceContext? resourceContext = null)
    {
        SetSpawnActorDefinition(actorDefinition);
        if (resourceContext is not null)
            SetResourceContext(resourceContext);
        var humans = Math.Max(0, spawn.HumanPlayerCount);
        var total = humans + spawn.AiPerFaction * 2;
        var hexes = SeededWorldGenerator.PickGrassSpawns(Grid, total, _random);
        var i = 0;

        for (var h = 0; h < humans; h++)
            AddActor(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[i++], HexSize));

        for (var a = 0; a < spawn.AiPerFaction; a++)
        {
            var ally = AddActor(spawn.PlayerFactionId, HexWorldLayout.ToWorld(hexes[i++], HexSize));
            AttachController(new AiController(_random), ally);
        }

        for (var a = 0; a < spawn.AiPerFaction; a++)
        {
            var rival = AddActor(spawn.RivalFactionId, HexWorldLayout.ToWorld(hexes[i++], HexSize));
            AttachController(new AiController(_random), rival);
        }
    }

    /// <summary>Full simulation step: passives → controllers → movement → projectiles → death prune.</summary>
    public void Tick(float dt)
    {
        if (dt <= 0f)
            return;

        TickActorPassives(dt);

        foreach (var c in _controllers)
            c.Tick(this, dt);

        ApplyMovement(dt);
        TickProjectiles(dt);
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

    /// <summary>Remove a living actor and detach its controller (player drop).</summary>
    public void ForceRemoveActor(Actor actor)
    {
        DetachControllersFor(actor);
        if (actor.Cell is { } cell)
            _actorsByCell.Remove(cell);
        actor.Cell = null;
        _actors.Remove(actor);
    }

    private void ClearCellAnchoredActors()
    {
        foreach (var actor in _actorsByCell.Values.ToList())
        {
            DetachControllersFor(actor);
            _actors.Remove(actor);
        }
        _actorsByCell.Clear();
    }

    private void ClearRivalsAndMissiles(int rivalFactionId)
    {
        _swingArcs.Clear();

        for (var i = _actors.Count - 1; i >= 0; i--)
        {
            var actor = _actors[i];
            if (actor.IsProjectile)
            {
                _actors.RemoveAt(i);
                continue;
            }

            if (actor.FactionId != rivalFactionId)
                continue;

            DetachControllersFor(actor);
            if (actor.Cell is { } cell)
                _actorsByCell.Remove(cell);
            actor.Cell = null;
            _actors.RemoveAt(i);
        }
    }

    private void RepositionAndHealHumans(SpawnConfig spawn)
    {
        var playerFaction = spawn.PlayerFactionId;
        var humans = _actors
            .Where(c => c.FactionId == playerFaction && c.Cell is null && !c.IsProjectile)
            .ToList();
        var needed = Math.Max(0, spawn.HumanPlayerCount);

        while (humans.Count < needed)
            humans.Add(AddActor(playerFaction, SimVec2.Zero));

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

    private void DetachControllersFor(Actor actor)
    {
        for (var c = _controllers.Count - 1; c >= 0; c--)
        {
            if (_controllers[c].Pawn?.Id != actor.Id)
                continue;
            _controllers[c].Unpossess();
            _controllers.RemoveAt(c);
        }
    }

    private void ApplyMovement(float dt)
    {
        foreach (var actor in _actors)
        {
            if (!actor.IsAlive || !actor.TryGetMoveEffect(out var move) || move is null)
                continue;

            var input = actor.MoveIntent;
            var displacement = SimVec2.Zero;
            if (input.LengthSquared >= 1e-10f)
            {
                var dir = input.Normalized();
                actor.Facing = dir;
                displacement = dir * (move.Speed * dt);
            }

            CollectFreeActorObstacles(actor.Id);
            actor.Position = CircleHexCollision.MoveAndSlide(
                actor.Position,
                displacement,
                PlayerRadius,
                _wallPolygons,
                _actorObstacleCenters,
                PlayerRadius);
        }
    }

    private void CollectFreeActorObstacles(int excludeActorId)
    {
        _actorObstacleCenters.Clear();
        foreach (var other in _actors)
        {
            if (!other.IsAlive || other.IsProjectile || other.Id == excludeActorId || other.Cell is not null)
                continue;
            if (!other.TryGetMoveEffect(out _))
                continue;
            _actorObstacleCenters.Add(other.Position);
        }
    }

    private void TickProjectiles(float dt)
    {
        for (var i = _actors.Count - 1; i >= 0; i--)
        {
            var actor = _actors[i];
            if (actor.Projectile is not { } flight)
                continue;

            var step = flight.Velocity * dt;
            actor.Position += step;
            flight.DistanceTraveled += step.Length;

            if (flight.DistanceTraveled >= flight.Range ||
                HitsWall(actor.Position, flight.Size) ||
                TryProjectileHitFreeActor(actor, flight) ||
                TryProjectileHitCellActor(actor, flight))
            {
                _actors.RemoveAt(i);
            }
        }
    }

    private bool TryProjectileHitFreeActor(Actor projectile, ProjectileFlight flight)
    {
        foreach (var actor in _actors)
        {
            if (!actor.IsAlive || actor.IsProjectile || actor.Cell is not null)
                continue;
            if (flight.OwnerActorId is int oid && oid == actor.Id)
                continue;
            if (!flight.FriendlyFire && !FactionRules.AreHostile(projectile.FactionId, actor.FactionId))
                continue;

            var delta = actor.Position - projectile.Position;
            var hitR = PlayerRadius + flight.Size;
            if (delta.LengthSquared <= hitR * hitR)
            {
                ApplyDamage(actor, flight.Damage);
                return true;
            }
        }

        return false;
    }

    private bool TryProjectileHitCellActor(Actor projectile, ProjectileFlight flight)
    {
        foreach (var (cell, actor) in _actorsByCell)
        {
            if (!actor.IsDestructible || !actor.IsAlive)
                continue;
            if (flight.OwnerActorId is int oid && oid == actor.Id)
                continue;
            if (!flight.FriendlyFire && !FactionRules.AreHostile(projectile.FactionId, actor.FactionId))
                continue;

            var center = HexWorldLayout.ToWorld(cell, HexSize);
            var delta = center - projectile.Position;
            var hitR = PlayerRadius + flight.Size;
            if (delta.LengthSquared <= hitR * hitR)
            {
                ApplyDamage(actor, flight.Damage);
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
        foreach (var actor in _actors)
        {
            if (!actor.IsAlive || actor.IsProjectile || actor.Cell is not null)
                continue;
            if (arc.OwnerCharacterId is int oid && oid == actor.Id)
                continue;
            if (!arc.FriendlyFire && !FactionRules.AreHostile(arc.OwnerFactionId, actor.FactionId))
                continue;

            if (Swing.IsPointInArc(
                    arc.Origin,
                    arc.Facing,
                    arc.Radius,
                    arc.ArcDegrees,
                    actor.Position,
                    PlayerRadius))
            {
                ApplyDamage(actor, arc.Damage);
            }
        }

        foreach (var (cell, actor) in _actorsByCell)
        {
            if (!actor.IsDestructible || !actor.IsAlive)
                continue;
            if (arc.OwnerCharacterId is int oid && oid == actor.Id)
                continue;
            if (!arc.FriendlyFire && !FactionRules.AreHostile(arc.OwnerFactionId, actor.FactionId))
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

    private bool HitsWall(SimVec2 position, float radius)
    {
        foreach (var wall in _wallPolygons)
        {
            if (CircleHexCollision.TryCircleConvex(position, radius, wall, out _, out var pen) && pen > 0f)
                return true;
        }

        return false;
    }

    private void RemoveDeadActors()
    {
        for (var i = _actors.Count - 1; i >= 0; i--)
        {
            if (_actors[i].IsAlive)
                continue;
            var dead = _actors[i];
            TryPlaceDeathDrops(dead);
            DetachControllersFor(dead);
            if (dead.Cell is { } cell)
                _actorsByCell.Remove(cell);
            dead.Cell = null;
            _actors.RemoveAt(i);
        }
    }

    private void TryPlaceDeathDrops(Actor dead)
    {
        foreach (var effect in dead.Effects)
        {
            if (effect is not IDeathDropEffect drop)
                continue;
            if (!TryGetActorDefinition(drop.ActorDefinitionId, out var actorDef) || actorDef is null)
                continue;

            var cell = dead.Cell ?? HexWorldLayout.WorldToAxial(dead.Position, HexSize);
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
