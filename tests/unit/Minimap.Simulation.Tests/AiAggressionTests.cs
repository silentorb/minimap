using Xunit;

namespace Minimap.Simulation.Tests;

public class AiAggressionTests
{
    [Fact]
    public void Aggression_zero_does_not_set_goal_to_distant_hostile()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var aiPawn = w.AddActor(2, new SimVec2(0f, 0f), TestContent.Zombie);
        var hostile = w.AddActor(1, new SimVec2(200f, 0f), TestContent.Bare);

        // Seeded RNG that always pauses roam (NextDouble < PauseChance).
        var ai = new AiController(new AlwaysPauseRandom(), new DirectMoveSteering(), aggression: 0f);
        w.AttachController(ai, aiPawn);

        ai.Tick(w, 0.016f);
        Assert.Equal(SimVec2.Zero, aiPawn.MoveIntent);
        Assert.Equal(0f, ai.Aggression);

        // Contrast: aggression 1 with same pause RNG still beelines.
        var chaser = w.AddActor(2, new SimVec2(0f, 20f), TestContent.Zombie);
        var chaseAi = new AiController(new AlwaysPauseRandom(), new DirectMoveSteering(), aggression: 1f);
        w.AttachController(chaseAi, chaser);
        chaseAi.Tick(w, 0.016f);
        Assert.True(chaser.MoveIntent.X > 0f);
        Assert.True((hostile.Position - chaser.Position).X > 0f);
    }

    [Fact]
    public void Aggression_zero_still_swings_when_hostile_in_reach()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var aiPawn = w.AddActor(2, new SimVec2(0f, 0f), TestContent.Zombie);
        var victim = w.AddActor(1, new SimVec2(w.HexSize * 0.5f, 0f), TestContent.Bare);

        var ai = new AiController(new Random(1), new DirectMoveSteering(), aggression: 0f);
        w.AttachController(ai, aiPawn);

        // Advance past swing cooldown randomization.
        for (var i = 0; i < 40; i++)
            ai.Tick(w, 0.1f);

        Assert.True(victim.Health < victim.MaxHealth || w.SwingArcs.Count > 0 || victim.Health == 0);
    }

    [Fact]
    public void High_aggression_beelines_toward_nearest_hostile()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var chaser = w.AddActor(2, new SimVec2(0f, 0f), TestContent.Zombie);
        w.AddActor(1, new SimVec2(80f, 0f), TestContent.Bare);

        var ai = new AiController(
            new Random(1),
            new DirectMoveSteering(),
            aggression: AiTuning.CrazedCarrotAggression);
        w.AttachController(ai, chaser);

        ai.Tick(w, 0.016f);
        Assert.True(chaser.MoveIntent.X > 0f);
    }

    [Fact]
    public void SpawnChaseActor_uses_crazed_aggression()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var spawned = w.SpawnChaseActor(
            TestContent.Zombie,
            new SimVec2(0f, 0f),
            factionId: 2);

        var ai = Assert.IsType<AiController>(
            Assert.Single(w.Controllers, c => c.Pawn?.Id == spawned.Id));
        Assert.Equal(AiTuning.CrazedCarrotAggression, ai.Aggression);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }

    /// <summary>NextDouble always below <see cref="AiWanderGoals.PauseChance"/> so roam pauses.</summary>
    private sealed class AlwaysPauseRandom : Random
    {
        public override double NextDouble() => 0.0;
    }
}
