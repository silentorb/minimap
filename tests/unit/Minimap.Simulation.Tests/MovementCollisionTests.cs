using Xunit;

namespace Minimap.Simulation.Tests;

public class MovementCollisionTests
{
    [Fact]
    public void Move_along_plus_x_increases_x()
    {
        var gen = new AllFloorAtOriginGenerator();
        var w = GameWorld.Create(3, 1, 1, gen);
        var before = w.Players[0].Position;
        w.SetPlayerInput(0, new SimVec2(1f, 0f));
        w.TickMovement(0.1f);
        Assert.True(w.Players[0].Position.X > before.X);
        Assert.Equal(before.Y, w.Players[0].Position.Y, precision: 3);
    }

    [Fact]
    public void Move_along_minus_y_decreases_y()
    {
        var gen = new AllFloorAtOriginGenerator();
        var w = GameWorld.Create(3, 1, 1, gen);
        var before = w.Players[0].Position;
        w.SetPlayerInput(0, new SimVec2(0f, -1f));
        w.TickMovement(0.1f);
        Assert.True(w.Players[0].Position.Y < before.Y);
        Assert.Equal(before.X, w.Players[0].Position.X, precision: 3);
    }

    [Fact]
    public void Head_on_into_wall_stalls()
    {
        var gen = new CorridorWithEastWallGenerator();
        var w = GameWorld.Create(2, 1, 1, gen);
        // Place player just left of the wall at (1,0), moving straight +X into it.
        var wallCenter = HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize);
        w.Players[0].Position = new SimVec2(wallCenter.X - w.HexSize - w.PlayerRadius - 0.5f, wallCenter.Y);

        w.SetPlayerInput(0, new SimVec2(1f, 0f));
        for (var i = 0; i < 60; i++)
            w.TickMovement(1f / 60f);

        var after = w.Players[0].Position;
        // Must not penetrate into the wall hex interior.
        Assert.False(
            CircleHexCollision.TryCircleConvex(
                after,
                w.PlayerRadius,
                HexWorldLayout.AbsoluteHexVertices(new HexAxial(1, 0), w.HexSize),
                out _,
                out var pen) && pen > 0.05f);
        // Head-on: little or no further progress once blocked.
        var blockedX = after.X;
        w.TickMovement(1f / 60f);
        Assert.Equal(blockedX, w.Players[0].Position.X, precision: 2);
    }

    [Fact]
    public void Angled_approach_slides_along_wall()
    {
        var gen = new CorridorWithEastWallGenerator();
        var w = GameWorld.Create(2, 1, 1, gen);
        var wallCenter = HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize);
        // Approach the vertical-ish face with +X and +Y so slide should change Y.
        w.Players[0].Position = new SimVec2(wallCenter.X - w.HexSize - w.PlayerRadius - 1f, wallCenter.Y - 4f);
        var before = w.Players[0].Position;

        w.SetPlayerInput(0, new SimVec2(1f, 1f));
        for (var i = 0; i < 90; i++)
            w.TickMovement(1f / 60f);

        var after = w.Players[0].Position;
        Assert.True(after.Y > before.Y + 1f, $"Expected slide along wall in Y; before={before} after={after}");
        Assert.False(
            CircleHexCollision.TryCircleConvex(
                after,
                w.PlayerRadius,
                HexWorldLayout.AbsoluteHexVertices(new HexAxial(1, 0), w.HexSize),
                out _,
                out var pen) && pen > 0.05f);
    }

    private sealed class AllFloorAtOriginGenerator : IWorldGenerator
    {
        public void Generate(HexGrid grid, Span<PlayerSlot> playersOut, Random random, float hexSize = HexWorldLayout.DefaultHexSize)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Floor);
            playersOut[0] = new PlayerSlot(0, HexWorldLayout.ToWorld(new HexAxial(0, 0), hexSize));
        }
    }

    /// <summary>Floor at origin and west; wall at (1,0) blocking east travel.</summary>
    private sealed class CorridorWithEastWallGenerator : IWorldGenerator
    {
        public void Generate(HexGrid grid, Span<PlayerSlot> playersOut, Random random, float hexSize = HexWorldLayout.DefaultHexSize)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Floor);
            grid.Set(new HexAxial(1, 0), CellType.Wall);
            playersOut[0] = new PlayerSlot(0, HexWorldLayout.ToWorld(new HexAxial(0, 0), hexSize));
        }
    }
}
