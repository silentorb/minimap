using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>
/// Authoritative playthrough session (headless): world, scenario pacing, players, game-over.
/// Neighbors feed constructed data in; Client attaches controllers separately.
/// </summary>
public sealed class GameSession
{
    private readonly List<Player> _players = new();
    private readonly SpawnConfig _spawnConfig;
    private readonly ScenarioRunner _scenarioRunner = new();
    private readonly List<int> _humanDeathsThisTick = new();
    private bool _isGameOver;

    private GameSession(
        GameWorld world,
        Random rng,
        Scenario scenario,
        SpawnConfig spawnConfig,
        GameContent content)
    {
        World = world;
        Rng = rng;
        Scenario = scenario;
        Content = content;
        _spawnConfig = spawnConfig;
    }

    public GameWorld World { get; }
    public Random Rng { get; }
    public Scenario Scenario { get; }
    public GameContent Content { get; }
    public ScenarioRunner ScenarioRunner => _scenarioRunner;
    public IReadOnlyList<Player> Players => _players;

    /// <summary>Characters currently bound to <see cref="Players"/> (order matches).</summary>
    public IReadOnlyList<Character> HumanPawns =>
        _players.Select(p => p.Character).Where(c => c is not null).Cast<Character>().ToList();

    public bool IsGameOver => _isGameOver;
    public ScenarioTickResult LastScenarioTickResult { get; private set; }

    /// <summary>Player indices whose character transitioned alive → dead during the last <see cref="Tick"/>.</summary>
    public IReadOnlyList<int> HumanDeathsThisTick => _humanDeathsThisTick;

    public static GameSession Create(
        int radiusX,
        int radiusY,
        int seed,
        float hexSize,
        SpawnConfig spawn,
        Scenario scenario,
        int localPlayerCount,
        GameContent content,
        int accessoryPoints = 0,
        IReadOnlyList<IReadOnlyList<AccessoryDefinition>>? selectedAccessoriesByPlayer = null)
    {
        ArgumentNullException.ThrowIfNull(content);

        var count = Math.Clamp(localPlayerCount, 1, 4);
        var config = new SpawnConfig
        {
            PlayerFactionId = spawn.PlayerFactionId,
            RivalFactionId = spawn.RivalFactionId,
            AiPerFaction = spawn.AiPerFaction,
            HumanPlayerCount = count,
        };

        var world = GameWorld.Create(radiusX, radiusY, seed, hexSize: hexSize);
        world.ApplyGameContent(content);

        var session = new GameSession(
            world,
            new Random(seed),
            scenario,
            config,
            content);

        session.CreatePlayers(count, accessoryPoints, selectedAccessoriesByPlayer);
        session.SpawnPlayers(config);
        // Spawner placement parked for sandbox play (see waves / scenarios feature docs).
        return session;
    }

    public void Tick(float dt)
    {
        _humanDeathsThisTick.Clear();
        if (_isGameOver)
            return;

        LastScenarioTickResult = _scenarioRunner.Tick(
            World,
            Scenario,
            _spawnConfig,
            Content.WorldSpawnerPool,
            dt);

        if (LastScenarioTickResult.LevelRegenerated)
            RelinkPlayerCharacters(_spawnConfig.PlayerFactionId);

        // Track pawns still in the world roster entering this tick (includes already-at-0
        // health from out-of-band damage; RemoveDeadActors runs inside World.Tick).
        var presentBefore = new bool[_players.Count];
        for (var i = 0; i < _players.Count; i++)
        {
            var character = _players[i].Character;
            presentBefore[i] = character is not null && World.Characters.Contains(character);
        }

        World.Tick(dt);

        for (var i = 0; i < _players.Count; i++)
        {
            if (!presentBefore[i])
                continue;
            var character = _players[i].Character;
            if (character is { IsAlive: true } && World.Characters.Contains(character))
                continue;
            _humanDeathsThisTick.Add(i);
        }

        if (AllHumanPawnsDead())
            _isGameOver = true;
    }

    /// <summary>Drop a human player mid-game (disconnect flow). Caller unbinds any client controller first.</summary>
    public bool DropHumanPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        var player = _players[playerIndex];
        var pawn = player.Character;
        player.Character = null;
        _players.RemoveAt(playerIndex);
        if (pawn is not null)
            World.ForceRemoveCharacter(pawn);
        return true;
    }

    private void CreatePlayers(
        int count,
        int accessoryPoints,
        IReadOnlyList<IReadOnlyList<AccessoryDefinition>>? selectedAccessoriesByPlayer)
    {
        _players.Clear();
        for (var i = 0; i < count; i++)
        {
            var player = new Player(i, accessoryPoints);
            if (selectedAccessoriesByPlayer is not null && i < selectedAccessoriesByPlayer.Count)
                player.SetSelectedAccessories(selectedAccessoriesByPlayer[i]);
            _players.Add(player);
        }
    }

    private void SpawnPlayers(SpawnConfig spawn)
    {
        if (_players.Count == 0)
            return;

        var hexes = SeededWorldGenerator.PickGrassSpawns(World.Grid, _players.Count, Rng);
        for (var i = 0; i < _players.Count; i++)
        {
            var character = World.AddCharacter(
                spawn.PlayerFactionId,
                HexWorldLayout.ToWorld(hexes[i], World.HexSize),
                Content.DefaultCharacter);

            foreach (var accessoryDef in _players[i].SelectedAccessories)
                character.AddAccessory(accessoryDef.CreateInstance());

            _players[i].Character = character;
        }
    }

    private void RelinkPlayerCharacters(int playerFactionId)
    {
        var humans = World.Characters.Where(c => c.FactionId == playerFactionId).ToList();
        for (var i = 0; i < _players.Count && i < humans.Count; i++)
            _players[i].Character = humans[i];
    }

    private bool AllHumanPawnsDead()
    {
        if (_players.Count == 0)
            return false;

        foreach (var player in _players)
        {
            if (player.Character is { IsAlive: true })
                return false;
        }

        return true;
    }
}
