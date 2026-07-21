using Minimap.Client;
using Minimap.Extensive;
using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.App;

/// <summary>Composition root: wires Simulation world with Client player controllers and HUD models.</summary>
public sealed class GameSession
{
    private readonly List<PlayerController> _players = new();
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
    public IReadOnlyList<PlayerController> Players => _players;
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
        session.AttachHumanPlayers(config.PlayerFactionId, count);
        return session;
    }

    public void SetMoveInput(int playerIndex, SimVec2 direction)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetMoveInput(direction);
    }

    public void Tick(float dt)
    {
        if (_isGameOver)
            return;

        LastScenarioTickResult = _scenarioRunner.Tick(World, Scenario, _spawnConfig, dt);
        if (LastScenarioTickResult.LevelRegenerated)
            ReattachHumanPlayers(_spawnConfig.PlayerFactionId);

        World.Tick(dt);

        if (AllHumanPlayersDead())
            _isGameOver = true;
    }

    public IReadOnlyList<PlayerHudModel> BuildHudModels()
    {
        var models = new List<PlayerHudModel>(_players.Count);
        for (var i = 0; i < _players.Count; i++)
        {
            var pawn = _players[i].Pawn;
            models.Add(new PlayerHudModel
            {
                DisplayName = $"Player {i + 1}",
                Health = pawn is { IsAlive: true } ? pawn.Health : 0f,
                MaxHealth = pawn?.MaxHealth ?? CombatTuning.DefaultMaxHealth,
            });
        }

        return models;
    }

    /// <summary>Drop a local human mid-game (disconnect flow).</summary>
    public bool DropHumanPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        var controller = _players[playerIndex];
        var pawn = controller.Pawn;
        controller.Unpossess();
        _players.RemoveAt(playerIndex);
        _humanPawns.RemoveAt(playerIndex);

        if (pawn is not null)
            World.ForceRemoveCharacter(pawn);

        return true;
    }

    private bool AllHumanPlayersDead()
    {
        if (_players.Count == 0)
            return false;

        foreach (var player in _players)
        {
            if (player.Pawn is { IsAlive: true })
                return false;
        }

        return true;
    }

    private void AttachHumanPlayers(int playerFactionId, int count)
    {
        var controlled = new HashSet<int>();
        foreach (var c in World.Controllers)
        {
            if (c.Pawn is { } pawn)
                controlled.Add(pawn.Id);
        }

        var humans = World.Characters
            .Where(c => c.FactionId == playerFactionId && !controlled.Contains(c.Id))
            .Take(count)
            .ToList();

        foreach (var human in humans)
        {
            var controller = new PlayerController();
            World.AttachController(controller, human);
            _players.Add(controller);
            _humanPawns.Add(human);
        }
    }

    private void ReattachHumanPlayers(int playerFactionId)
    {
        var controlled = new HashSet<int>();
        foreach (var c in World.Controllers)
        {
            if (c.Pawn is { } pawn)
                controlled.Add(pawn.Id);
        }

        var humans = World.Characters
            .Where(c => c.FactionId == playerFactionId && !controlled.Contains(c.Id))
            .Take(_players.Count)
            .ToList();

        for (var i = 0; i < _players.Count && i < humans.Count; i++)
        {
            var controller = _players[i];
            if (controller.Pawn is not null)
                controller.Unpossess();

            World.AttachController(controller, humans[i]);
            _humanPawns[i] = humans[i];
        }
    }
}
