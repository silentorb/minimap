using Xunit;

namespace Minimap.Simulation.Tests;

public class AiWanderAndSteeringTests
{
    [Fact]
    public void DirectMoveSteering_moves_toward_goal()
    {
        var steering = new DirectMoveSteering(arriveDistance: 1f);
        var pawn = new Character(0, 1, SimVec2.Zero, TestContent.Generic);
        steering.SetGoal(new SimVec2(40f, 0f));

        var intent = steering.SampleMoveIntent(pawn, 0.016f);
        Assert.True(intent.X > 0.9f);
        Assert.True(MathF.Abs(intent.Y) < 0.1f);
    }

    [Fact]
    public void DirectMoveSteering_clears_to_zero_intent()
    {
        var steering = new DirectMoveSteering();
        var pawn = new Character(0, 1, SimVec2.Zero, TestContent.Generic);
        steering.SetGoal(new SimVec2(40f, 0f));
        steering.ClearGoal();
        Assert.Equal(SimVec2.Zero, steering.SampleMoveIntent(pawn, 0.016f));
    }

    [Fact]
    public void DirectMoveSteering_arrives_with_zero_intent()
    {
        var steering = new DirectMoveSteering(arriveDistance: 10f);
        var pawn = new Character(0, 1, new SimVec2(5f, 0f), TestContent.Generic);
        steering.SetGoal(new SimVec2(8f, 0f));
        Assert.Equal(SimVec2.Zero, steering.SampleMoveIntent(pawn, 0.016f));
    }

    [Fact]
    public void TryPickRandomGrassWorld_returns_grass_cell_center()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(2, 2, 1, gen);
        var goal = AiWanderGoals.TryPickRandomGrassWorld(w, new Random(7));
        Assert.NotNull(goal);

        var axial = HexWorldLayout.WorldToAxial(goal.Value, w.HexSize);
        Assert.Equal(CellType.Grass, w.Grid.Get(axial));
        Assert.Equal(HexWorldLayout.ToWorld(axial, w.HexSize), goal.Value);
    }

    [Fact]
    public void PickGrassGoalOrPause_can_pause()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(2, 2, 1, gen);
        var goal = AiWanderGoals.PickGrassGoalOrPause(w, new FixedDoubleRandom(0.1));
        Assert.Null(goal);
    }

    [Fact]
    public void PickGrassGoalOrPause_can_pick_grass()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(2, 2, 1, gen);
        var goal = AiWanderGoals.PickGrassGoalOrPause(w, new FixedDoubleRandom(0.5));
        Assert.NotNull(goal);
    }

    [Fact]
    public void AiController_with_direct_steering_writes_intent_toward_goal()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(3, 3, 1, gen);
        w.SetSpawnCharacterDefinition(TestContent.Generic);
        var pawn = w.AddCharacter(1, SimVec2.Zero);
        var steering = new DirectMoveSteering(arriveDistance: 1f);
        var ai = new AiController(new FixedDoubleRandom(0.5), steering);
        w.AttachController(ai, pawn);

        // First tick picks a floor goal (no pause); second tick samples intent before retarget.
        w.Tick(0.016f);
        Assert.IsType<DirectMoveSteering>(ai.Steering);
        var intent = pawn.MoveIntent;
        Assert.True(intent.LengthSquared > 1e-6f, "Expected non-zero move intent toward floor goal.");
    }

    [Fact]
    public void AiController_ReplaceSteering_swaps_backend()
    {
        var gen = new AllGrassGenerator();
        var w = GameWorld.Create(2, 2, 1, gen);
        w.SetSpawnCharacterDefinition(TestContent.Generic);
        var pawn = w.AddCharacter(1, SimVec2.Zero);
        var ai = new AiController(new Random(1));
        w.AttachController(ai, pawn);
        Assert.IsType<DirectMoveSteering>(ai.Steering);

        var replacement = new DirectMoveSteering();
        ai.ReplaceSteering(replacement);
        Assert.Same(replacement, ai.Steering);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }

    /// <summary>Deterministic Random: fixed NextDouble; Next(max) always 0.</summary>
    private sealed class FixedDoubleRandom : Random
    {
        private readonly double _nextDouble;

        public FixedDoubleRandom(double nextDouble) => _nextDouble = nextDouble;

        public override double NextDouble() => _nextDouble;

        public override int Next(int maxValue) => 0;
    }
}
