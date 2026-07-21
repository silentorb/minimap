using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>
/// Authoritative playthrough session (headless): world, scenario pacing, human pawns, game-over.
/// Neighbors feed constructed data in; Client attaches controllers separately.
/// </summary>
public sealed class GameSession
{
    private readonly List<Character> _humanPawns = new();
    private readonly SpawnConfig _spawnConfig;
    private readonly ScenarioRunner _scenarioRunner = new();
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
    public IReadOnlyList<Character> HumanPawns => _humanPawns;
    public bool IsGameOver => _isGameOver;
    public ScenarioTickResult LastScenarioTickResult { get; private set; }

    public static GameSession Create(
        int radiusX,
        int radiusY,
        int seed,
        float hexSize,
        SpawnConfig spawn,
        Scenario scenario,
        int localPlayerCount,
        GameContent content)
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
        world.InitializeScenarioLevel(scenario, config, content.DefaultCharacter);

        var session = new GameSession(
            world,
            new Random(seed),
            scenario,
            config,
            content);
        session.CaptureHumanPawns(config.PlayerFactionId, count);
        return session;
    }

    public void Tick(float dt)
    {
        if (_isGameOver)
            return;

        LastScenarioTickResult = _scenarioRunner.Tick(World, Scenario, _spawnConfig, dt);
        if (LastScenarioTickResult.LevelRegenerated)
            CaptureHumanPawns(_spawnConfig.PlayerFactionId, _humanPawns.Count);

        World.Tick(dt);

        if (AllHumanPawnsDead())
            _isGameOver = true;
    }

    /// <summary>Drop a human pawn mid-game (disconnect flow). Caller unbinds any client controller first.</summary>
    public bool DropHumanPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _humanPawns.Count)
            return false;

        var pawn = _humanPawns[playerIndex];
        _humanPawns.RemoveAt(playerIndex);
        World.ForceRemoveCharacter(pawn);
        return true;
    }

    private bool AllHumanPawnsDead()
    {
        if (_humanPawns.Count == 0)
            return false;

        foreach (var pawn in _humanPawns)
        {
            if (pawn.IsAlive)
                return false;
        }

        return true;
    }

    private void CaptureHumanPawns(int playerFactionId, int count)
    {
        _humanPawns.Clear();
        foreach (var character in World.Characters)
        {
            if (character.FactionId != playerFactionId)
                continue;
            _humanPawns.Add(character);
            if (_humanPawns.Count >= count)
                break;
        }
    }
}
