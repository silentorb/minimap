using Xunit;

namespace Minimap.Simulation.Tests;

public class ShootFireGateTests
{
    [Fact]
    public void Tick_does_not_fire_without_wantsFire()
    {
        var (world, driver, pawn) = TestWorldHelpers.CreateDriven(4, 4, 11);
        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetFireHeld(false);

        for (var i = 0; i < 5; i++)
            world.Tick(1f);

        Assert.Empty(TestWorldHelpers.Projectiles(world));
        Assert.Same(pawn, driver.Pawn);
    }

    [Fact]
    public void Tick_fires_when_wantsFire_and_aim_ready()
    {
        var (world, driver, _) = TestWorldHelpers.CreateDriven(4, 4, 12);
        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetFireHeld(true);

        world.Tick(0.016f);
        Assert.NotEmpty(TestWorldHelpers.Projectiles(world));
    }
}
