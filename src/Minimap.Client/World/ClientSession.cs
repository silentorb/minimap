using Minimap.Simulation;

namespace Minimap.Client.World;

/// <summary>
/// Local client adapter over a Simulation <see cref="GameSession"/>:
/// PlayerControllers, input feed, and HUD model mapping.
/// </summary>
public sealed class ClientSession
{
    private readonly GameSession _session;
    private readonly List<PlayerController> _players = new();

    public ClientSession(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
        AttachHumanPlayers();
    }

    public GameSession Simulation => _session;
    public IReadOnlyList<PlayerController> Players => _players;

    public void SetMoveInput(int playerIndex, SimVec2 direction)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetMoveInput(direction);
    }

    public void SetAimInput(int playerIndex, SimVec2 direction)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return;
        _players[playerIndex].SetAimInput(direction);
    }

    public void OnLevelRegenerated() => ReattachHumanPlayers();

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

    /// <summary>Unpossess, drop from simulation, and remove local controller.</summary>
    public bool DropHumanPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _players.Count)
            return false;

        var controller = _players[playerIndex];
        controller.Unpossess();
        _players.RemoveAt(playerIndex);
        return _session.DropHumanPlayer(playerIndex);
    }

    private void AttachHumanPlayers()
    {
        foreach (var human in _session.HumanPawns)
        {
            var controller = new PlayerController();
            _session.World.AttachController(controller, human);
            _players.Add(controller);
        }
    }

    private void ReattachHumanPlayers()
    {
        var humans = _session.HumanPawns;
        for (var i = 0; i < _players.Count && i < humans.Count; i++)
        {
            var controller = _players[i];
            if (controller.Pawn is not null)
                controller.Unpossess();

            _session.World.AttachController(controller, humans[i]);
        }
    }
}
