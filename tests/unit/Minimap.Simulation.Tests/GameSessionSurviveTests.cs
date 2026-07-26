using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class GameSessionSurviveTests
{
    [Fact]
    public void Crossing_survive_threshold_reports_once()
    {
        var session = CreateSession(1);
        Assert.Empty(session.SurviveFiveMinutesThisTick);

        session.Tick(GameSession.SurviveFiveMinutesSeconds - 0.1f);
        Assert.Empty(session.SurviveFiveMinutesThisTick);
        Assert.True(session.GetConsecutiveAliveSeconds(0) >= GameSession.SurviveFiveMinutesSeconds - 0.1f);

        session.Tick(0.2f);
        Assert.Equal(new[] { 0 }, session.SurviveFiveMinutesThisTick);

        session.Tick(1f);
        Assert.Empty(session.SurviveFiveMinutesThisTick);
    }

    [Fact]
    public void Death_resets_consecutive_alive_timer()
    {
        var session = CreateSession(1);
        session.Tick(10f);
        Assert.True(session.GetConsecutiveAliveSeconds(0) >= 10f);

        var pawn = session.HumanPawns[0];
        session.World.ApplyDamage(pawn, CombatTuning.DefaultMaxHealth);
        session.Tick(0.016f);

        Assert.Equal(0f, session.GetConsecutiveAliveSeconds(0));
        Assert.Empty(session.SurviveFiveMinutesThisTick);
    }

    private static GameSession CreateSession(int players)
    {
        var scenario = Scenario.Defaults;
        var spawn = new SpawnConfig { PlayerFactionId = 1, RivalFactionId = 2 };
        return GameSession.Create(
            4, 4, 42, HexWorldLayout.DefaultHexSize, spawn, scenario, players, TestContent.Content);
    }
}
