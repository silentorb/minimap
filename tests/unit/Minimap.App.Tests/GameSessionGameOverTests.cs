using CompuQuest.Minimap;
using Minimap.Simulation;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class GameSessionGameOverTests
{
    private static GameContent TestGameContent()
    {
        var gun = new AccessoryDefinition(
            "gun",
            [
                new ShootEffect(
                    CombatTuning.FireIntervalSeconds,
                    CombatTuning.MissileSpeed,
                    CombatTuning.MissileDamage),
            ]);
        return new GameContent(new CharacterDefinition("generic", [gun]));
    }

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
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, TestGameContent());

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
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, TestGameContent());

        session.Tick(0.016f);

        Assert.False(session.IsGameOver);
    }

    [Fact]
    public void Create_stores_provided_game_content()
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var content = TestGameContent();
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, content);

        Assert.Same(content, session.Content);
        Assert.Equal("generic", session.Content.DefaultCharacter.Id);
    }
}
