using Xunit;

namespace Minimap.Simulation.Tests;

public class MovementCollisionTests
{
    private static SpawnConfig Solo => new() { AiPerFaction = 0 };

    [Fact]
    public void Move_along_plus_x_increases_x()
    {
        var gen = new AllFloorGenerator();
        var w = GameWorld.Create(3, 3, 1, gen, spawn: Solo);
        var pawn = w.PlayerController!.Pawn!;
        var before = pawn.Position;
        w.PlayerController.SetMoveInput(new SimVec2(1f, 0f));
        w.Tick(0.1f);
        Assert.True(pawn.Position.X > before.X);
        Assert.Equal(before.Y, pawn.Position.Y, precision: 3);
    }

    [Fact]
    public void Move_along_minus_y_decreases_y()
    {
        var gen = new AllFloorGenerator();
        var w = GameWorld.Create(3, 3, 1, gen, spawn: Solo);
        var pawn = w.PlayerController!.Pawn!;
        var before = pawn.Position;
        w.PlayerController.SetMoveInput(new SimVec2(0f, -1f));
        w.Tick(0.1f);
        Assert.True(pawn.Position.Y < before.Y);
        Assert.Equal(before.X, pawn.Position.X, precision: 3);
    }

    [Fact]
    public void Head_on_into_wall_stalls()
    {
        var gen = new CorridorWithEastWallGenerator();
        var w = GameWorld.Create(2, 2, 1, gen, spawn: Solo);
        var pawn = w.PlayerController!.Pawn!;
        var wallCenter = HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize);
        pawn.Position = new SimVec2(wallCenter.X - w.HexSize - w.PlayerRadius - 0.5f, wallCenter.Y);

        pawn.MoveIntent = new SimVec2(1f, 0f);
        for (var i = 0; i < 60; i++)
            w.TickMovement(1f / 60f);

        var after = pawn.Position;
        Assert.False(
            CircleHexCollision.TryCircleConvex(
                after,
                w.PlayerRadius,
                HexWorldLayout.AbsoluteHexVertices(new HexAxial(1, 0), w.HexSize),
                out _,
                out var pen) && pen > 0.05f);
        var blockedX = after.X;
        w.TickMovement(1f / 60f);
        Assert.Equal(blockedX, pawn.Position.X, precision: 2);
    }

    [Fact]
    public void Angled_approach_slides_along_wall()
    {
        var gen = new CorridorWithEastWallGenerator();
        var w = GameWorld.Create(2, 2, 1, gen, spawn: Solo);
        var pawn = w.PlayerController!.Pawn!;
        var wallCenter = HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize);
        pawn.Position = new SimVec2(wallCenter.X - w.HexSize - w.PlayerRadius - 1f, wallCenter.Y - 4f);
        var before = pawn.Position;

        pawn.MoveIntent = new SimVec2(1f, 1f);
        for (var i = 0; i < 90; i++)
            w.TickMovement(1f / 60f);

        var after = pawn.Position;
        Assert.True(after.Y > before.Y + 1f, $"Expected slide along wall in Y; before={before} after={after}");
        Assert.False(
            CircleHexCollision.TryCircleConvex(
                after,
                w.PlayerRadius,
                HexWorldLayout.AbsoluteHexVertices(new HexAxial(1, 0), w.HexSize),
                out _,
                out var pen) && pen > 0.05f);
    }

    private sealed class AllFloorGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Floor);
        }
    }

    private sealed class CorridorWithEastWallGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Floor);
            grid.Set(new HexAxial(1, 0), CellType.Wall);
        }
    }
}
