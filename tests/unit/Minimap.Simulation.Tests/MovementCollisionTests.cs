using Xunit;

namespace Minimap.Simulation.Tests;

public class MovementCollisionTests
{
    private static SpawnConfig Solo => new() { AiPerFaction = 0 };

    [Fact]
    public void Move_along_plus_x_increases_x()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, pawn) = TestWorldHelpers.CreateDriven(3, 3, 1, gen, Solo);
        var before = pawn.Position;
        driver.SetMoveInput(new SimVec2(1f, 0f));
        w.Tick(0.1f);
        Assert.True(pawn.Position.X > before.X);
        Assert.Equal(before.Y, pawn.Position.Y, precision: 3);
    }

    [Fact]
    public void Move_along_minus_y_decreases_y()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, pawn) = TestWorldHelpers.CreateDriven(3, 3, 1, gen, Solo);
        var before = pawn.Position;
        driver.SetMoveInput(new SimVec2(0f, -1f));
        w.Tick(0.1f);
        Assert.True(pawn.Position.Y < before.Y);
        Assert.Equal(before.X, pawn.Position.X, precision: 3);
    }

    [Fact]
    public void Head_on_into_wall_stalls()
    {
        var gen = new CorridorWithEastWallGenerator();
        var (w, _, pawn) = TestWorldHelpers.CreateDriven(2, 2, 1, gen, Solo);
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
        var (w, _, pawn) = TestWorldHelpers.CreateDriven(2, 2, 1, gen, Solo);
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

    [Fact]
    public void Head_on_into_other_character_stalls_without_overlap()
    {
        var gen = new AllGrassGenerator();
        var (w, _, pawn) = TestWorldHelpers.CreateDriven(3, 3, 1, gen, Solo);
        var blocker = w.AddActor(2, new SimVec2(40f, 0f));
        pawn.Position = new SimVec2(0f, 0f);
        blocker.Position = new SimVec2(w.PlayerRadius * 2f + 8f, 0f);

        pawn.MoveIntent = new SimVec2(1f, 0f);
        for (var i = 0; i < 120; i++)
            w.TickMovement(1f / 60f);

        var delta = pawn.Position - blocker.Position;
        var minDist = w.PlayerRadius * 2f;
        Assert.True(
            delta.LengthSquared >= (minDist - 0.05f) * (minDist - 0.05f),
            $"Expected no overlap; dist={MathF.Sqrt(delta.LengthSquared)} min={minDist}");
        var blockedX = pawn.Position.X;
        w.TickMovement(1f / 60f);
        Assert.Equal(blockedX, pawn.Position.X, precision: 2);
    }

    [Fact]
    public void Angled_approach_slides_around_other_character()
    {
        var gen = new AllGrassGenerator();
        var (w, _, pawn) = TestWorldHelpers.CreateDriven(3, 3, 1, gen, Solo);
        var blocker = w.AddActor(2, new SimVec2(0f, 0f));
        pawn.Position = new SimVec2(-(w.PlayerRadius * 2f + 6f), -8f);
        blocker.Position = new SimVec2(0f, 0f);
        var before = pawn.Position;

        pawn.MoveIntent = new SimVec2(1f, 1f);
        for (var i = 0; i < 120; i++)
            w.TickMovement(1f / 60f);

        var after = pawn.Position;
        Assert.True(after.Y > before.Y + 1f, $"Expected slide past other character in Y; before={before} after={after}");
        var delta = after - blocker.Position;
        var minDist = w.PlayerRadius * 2f;
        Assert.True(
            delta.LengthSquared >= (minDist - 0.05f) * (minDist - 0.05f),
            $"Expected no overlap; dist={MathF.Sqrt(delta.LengthSquared)} min={minDist}");
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }

    private sealed class CorridorWithEastWallGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
            grid.Set(new HexAxial(1, 0), CellType.Wall);
        }
    }
}
