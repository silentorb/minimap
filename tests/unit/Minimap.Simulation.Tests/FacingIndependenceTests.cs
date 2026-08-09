using Xunit;

using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

public class FacingIndependenceTests
{
    [Fact]
    public void ApplyMovement_does_not_change_facing()
    {
        var w = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        w.ApplyGameContent(TestContent.Content);
        var actor = w.AddActor(1, SimVec2.Zero, TestContent.Generic);
        actor.Facing = new SimVec2(0f, 1f);
        actor.MoveIntent = new SimVec2(1f, 0f);

        w.TickMovement(0.1f);

        Assert.Equal(0f, actor.Facing.X, precision: 4);
        Assert.Equal(1f, actor.Facing.Y, precision: 4);
        Assert.True(actor.Position.X > 0f);
    }

    [Fact]
    public void DriveController_updates_facing_from_aim_not_move()
    {
        var (w, driver, pawn) = TestWorldHelpers.CreateDriven(3, 3, 1);
        pawn.Facing = new SimVec2(1f, 0f);
        driver.SetMoveInput(new SimVec2(0f, 1f));
        driver.SetAimInput(new SimVec2(-1f, 0f));

        w.Tick(0.05f);

        Assert.Equal(-1f, pawn.Facing.X, precision: 4);
        Assert.Equal(0f, pawn.Facing.Y, precision: 4);
    }

    [Fact]
    public void DriveController_keeps_facing_when_aim_is_zero_while_moving()
    {
        var (w, driver, pawn) = TestWorldHelpers.CreateDriven(3, 3, 1);
        pawn.Facing = new SimVec2(0f, -1f);
        driver.SetMoveInput(new SimVec2(1f, 0f));
        driver.SetAimInput(SimVec2.Zero);

        w.Tick(0.05f);

        Assert.Equal(0f, pawn.Facing.X, precision: 4);
        Assert.Equal(-1f, pawn.Facing.Y, precision: 4);
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
