using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

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
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, TestContent.Content);

        var pawn = session.HumanPawns[0];
        session.World.ApplyDamage(pawn, CombatTuning.DefaultMaxHealth);
        session.Tick(0.016f);

        Assert.True(session.IsGameOver);
        Assert.Equal(new[] { 0 }, session.HumanDeathsThisTick);
    }

    [Fact]
    public void Drop_human_player_does_not_report_a_death()
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 2, TestContent.Content);

        Assert.True(session.DropHumanPlayer(0));
        session.Tick(0.016f);
        Assert.Empty(session.HumanDeathsThisTick);
    }

    [Fact]
    public void Surviving_human_player_does_not_trigger_game_over()
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, TestContent.Content);

        session.Tick(0.016f);

        Assert.False(session.IsGameOver);
    }

    [Fact]
    public void Create_stores_provided_game_content()
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var content = TestContent.Content;
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, content);

        Assert.Same(content, session.Content);
        Assert.Equal("generic", session.Content.DefaultActor.Id);
    }

    [Fact]
    public void Create_places_destructible_spawner_actors()
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        var session = GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, 1, TestContent.Content);

        Assert.Empty(session.World.Spawners);
        Assert.False(session.ScenarioRunner.Enabled);
        Assert.Equal(2, scenario.SpawnerCount);
        var spawners = session.World.CellActors.Values
            .Where(a => a.Definition.Id == AiTuning.ZombieSpawnerActorId)
            .ToList();
        Assert.Equal(2, spawners.Count);
        Assert.All(spawners, a =>
        {
            Assert.True(a.IsDestructible);
            Assert.Equal(400, a.MaxHealth);
            Assert.Equal(400, a.Health);
        });
    }
}
