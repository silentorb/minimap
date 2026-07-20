using Minimap.Simulation;
using Xunit;

namespace Minimap.App.Tests;

public class GameSessionGameOverTests
{
    [Fact]
    public void All_human_players_dead_sets_game_over()
    {
        var scenario = new Scenario
        {
            PreparationDuration = 100f,
            WaveCount = 1,
            WaveDuration = 100f,
            SpawnerCount = 1,
            SpawnerVolume = 1,
        };
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var session = GameSession.Create(4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1);

        var pawn = session.HumanPawns[0];
        session.World.ApplyDamage(pawn, CombatTuning.DefaultMaxHealth);
        session.Tick(0.016f);

        Assert.True(session.IsGameOver);
    }

    [Fact]
    public void Surviving_human_player_does_not_trigger_game_over()
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var session = GameSession.Create(4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1);

        session.Tick(0.016f);

        Assert.False(session.IsGameOver);
    }
}
