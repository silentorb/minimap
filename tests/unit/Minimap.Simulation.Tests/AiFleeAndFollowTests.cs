using Xunit;

namespace Minimap.Simulation.Tests;

public class AiFleeAndFollowTests
{
    [Fact]
    public void Seriously_injured_low_aggression_starts_flee_away_from_hostile()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var aiPawn = w.AddActor(2, new SimVec2(40f, 0f), TestContent.Zombie);
        aiPawn.Health = (int)(aiPawn.MaxHealth * AiTuning.SeriousInjuryHealthFraction);
        w.AddActor(1, new SimVec2(0f, 0f), TestContent.Bare);

        // NextDouble 0 → always start flee when aggression 0; also pauses roam after flee ends.
        var ai = new AiController(new FixedDoubleRandom(0.0), new DirectMoveSteering(), aggression: 0f);
        w.AttachController(ai, aiPawn);

        ai.Tick(w, 0.016f);
        Assert.True(ai.FleeRemaining > 0f);
        Assert.True(aiPawn.MoveIntent.X > 0f);
    }

    [Fact]
    public void Seriously_injured_max_aggression_does_not_flee()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var aiPawn = w.AddActor(2, new SimVec2(0f, 0f), TestContent.Zombie);
        aiPawn.Health = (int)(aiPawn.MaxHealth * AiTuning.SeriousInjuryHealthFraction);
        w.AddActor(1, new SimVec2(80f, 0f), TestContent.Bare);

        var ai = new AiController(new FixedDoubleRandom(0.0), new DirectMoveSteering(), aggression: 1f);
        w.AttachController(ai, aiPawn);

        ai.Tick(w, 0.016f);
        Assert.Equal(0f, ai.FleeRemaining);
        Assert.True(aiPawn.MoveIntent.X > 0f);
    }

    [Fact]
    public void Unowned_flee_ends_then_resumes_aggression_chase()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var aiPawn = w.AddActor(2, new SimVec2(40f, 0f), TestContent.Zombie);
        aiPawn.Health = (int)(aiPawn.MaxHealth * AiTuning.SeriousInjuryHealthFraction);
        w.AddActor(1, new SimVec2(0f, 0f), TestContent.Bare);

        // Possess swing cooldown, retarget delays, flee start (0), then fail re-flee (0.9) + pause roam (0).
        var ai = new AiController(
            new SequenceDoubleRandom(0.0, 0.0, 0.0, 0.0, 0.9, 0.0, 0.0, 0.9, 0.0),
            new DirectMoveSteering(),
            aggression: 0.5f);
        w.AttachController(ai, aiPawn);

        ai.Tick(w, 0.016f);
        Assert.True(ai.FleeRemaining > 0f);
        Assert.True(aiPawn.MoveIntent.X > 0f);

        // Drain flee timer and force a retarget after resume.
        ai.Tick(w, AiTuning.FleeDurationSeconds + 0.1f);
        ai.Tick(w, AiWanderGoals.RetargetMaxSeconds + 0.1f);

        Assert.Equal(0f, ai.FleeRemaining);
        // After resume with mid aggression and failed re-flee, pull toward hostile (negative X).
        Assert.True(aiPawn.MoveIntent.X < 0f);
    }

    [Fact]
    public void Owned_aggression_zero_near_owner_stays_still()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var owner = w.AddActor(1, new SimVec2(0f, 0f), TestContent.Bare);
        var companion = w.AddActor(1, new SimVec2(w.HexSize * 0.5f, 0f), TestContent.Zombie);
        companion.OwnerActorId = owner.Id;
        w.AddActor(2, new SimVec2(200f, 0f), TestContent.Bare);

        var ai = new AiController(new FixedDoubleRandom(0.0), new DirectMoveSteering(), aggression: 0f);
        w.AttachController(ai, companion);

        ai.Tick(w, 0.016f);
        Assert.Equal(SimVec2.Zero, companion.MoveIntent);
    }

    [Fact]
    public void Owned_aggression_one_beelines_toward_hostile()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var owner = w.AddActor(1, new SimVec2(0f, 0f), TestContent.Bare);
        var companion = w.AddActor(1, new SimVec2(w.HexSize * 0.5f, 0f), TestContent.Zombie);
        companion.OwnerActorId = owner.Id;
        w.AddActor(2, new SimVec2(80f, 0f), TestContent.Bare);

        var ai = new AiController(new FixedDoubleRandom(0.0), new DirectMoveSteering(), aggression: 1f);
        w.AttachController(ai, companion);

        ai.Tick(w, 0.016f);
        Assert.True(companion.MoveIntent.X > 0f);
    }

    [Fact]
    public void Owned_injured_flee_runs_toward_owner()
    {
        var w = GameWorld.Create(5, 5, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);

        var owner = w.AddActor(1, new SimVec2(100f, 0f), TestContent.Bare);
        var companion = w.AddActor(1, new SimVec2(40f, 0f), TestContent.Zombie);
        companion.OwnerActorId = owner.Id;
        companion.Health = (int)(companion.MaxHealth * AiTuning.SeriousInjuryHealthFraction);
        // Hostile on the opposite side — unowned flee would go left; owned flee goes to owner (right).
        w.AddActor(2, new SimVec2(0f, 0f), TestContent.Bare);

        var ai = new AiController(new FixedDoubleRandom(0.0), new DirectMoveSteering(), aggression: 0f);
        w.AttachController(ai, companion);

        ai.Tick(w, 0.016f);
        Assert.True(ai.FleeRemaining > 0f);
        Assert.True(companion.MoveIntent.X > 0f);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }

    private sealed class FixedDoubleRandom : Random
    {
        private readonly double _value;

        public FixedDoubleRandom(double value) => _value = value;

        public override double NextDouble() => _value;
    }

    private sealed class SequenceDoubleRandom : Random
    {
        private readonly double[] _values;
        private int _index;

        public SequenceDoubleRandom(params double[] values) => _values = values;

        public override double NextDouble()
        {
            var v = _values[Math.Min(_index, _values.Length - 1)];
            _index++;
            return v;
        }
    }
}
