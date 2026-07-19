using Minimap.Client;
using Minimap.Simulation;

namespace Minimap.App;

/// <summary>Composition root: wires Simulation world with Client player controllers and HUD models.</summary>
public sealed class GameSession
{
    private readonly List<PlayerController> _players = new();
    private readonly List<Character> _humanPawns = new();

    private GameSession(GameWorld world, Random rng)
    {
        World = world;
        Rng = rng;
    }

    public GameWorld World { get; }
    public Random Rng { get; }
    public IReadOnlyList<PlayerController> Players => _players;
    public IReadOnlyList<Character> HumanPawns => _humanPawns;

    public static GameSession Create(
        int radiusX,
        int radiusY,
        int seed,
        float hexSize,
        SpawnConfig spawn,
        int localPlayerCount)
    {
        var count = Math.Clamp(localPlayerCount, 1, 4);
        var config = new SpawnConfig
        {
            PlayerFactionId = spawn.PlayerFactionId,
            RivalFactionId = spawn.RivalFactionId,
            AiPerFaction = spawn.AiPerFaction,
            HumanPlayerCount = count,
        };

        var world = GameWorld.Create(radiusX, radiusY, seed, hexSize: hexSize, spawn: config);
        var session = new GameSession(world, new Random(seed));
        session.AttachHumanPlayers(config.PlayerFactionId, count);
        return session;
    }

    public void SetMoveInput(int playerIndex, SimVec2 direction)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetMoveInput(direction);
    }

    public void Tick(float dt) => World.Tick(dt);

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
                MaxHealth = pawn?.MaxHealth ?? 0f,
            });
        }

        return models;
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
}
