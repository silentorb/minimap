using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

public class MoveGatingTests
{
    [Fact]
    public void ApplyMovement_no_ops_without_move_effect()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var stationary = new ActorDefinition(
            "stationary",
            accessories: null,
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 100),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 100),
            ]);
        var actor = w.AddActor(1, SimVec2.Zero, stationary);
        var before = actor.Position;
        actor.MoveIntent = new SimVec2(1f, 0f);
        w.TickMovement(0.1f);
        Assert.Equal(before.X, actor.Position.X, precision: 4);
        Assert.Equal(before.Y, actor.Position.Y, precision: 4);
    }

    [Fact]
    public void ApplyMovement_uses_move_effect_speed()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var fastMove = new AccessoryDefinition("fast_move", [new TestMoveEffect(200f)]);
        var def = new ActorDefinition(
            "sprinter",
            [fastMove],
            resources:
            [
                new ActorResourceAmount(TestContent.MaxHealthResource.Tag, 100),
                new ActorResourceAmount(TestContent.HealthResource.Tag, 100),
            ]);
        var actor = w.AddActor(1, SimVec2.Zero, def);
        actor.MoveIntent = new SimVec2(1f, 0f);
        w.TickMovement(0.1f);
        Assert.Equal(20f, actor.Position.X, precision: 2);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }
}
