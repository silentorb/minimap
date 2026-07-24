using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class CharacterAccessoryTests
{
    [Fact]
    public void Instantiate_from_definition_adds_accessories_and_caches_effects()
    {
        var c = new Character(0, 1, SimVec2.Zero, TestContent.Generic, TestContent.ResourceContext);
        Assert.Single(c.Accessories);
        Assert.Equal("gun", c.Accessories[0].Definition.Id);
        Assert.Equal(2, c.Effects.Count);
        Assert.IsType<TestGrantResourceEffect>(c.Effects[0]);
        Assert.IsType<TestShootEffect>(c.Effects[1]);
    }

    [Fact]
    public void Add_and_remove_accessory_syncs_effect_cache()
    {
        var c = new Character(0, 1, SimVec2.Zero, TestContent.Bare, TestContent.ResourceContext);
        Assert.Empty(c.Effects);

        var gun = TestContent.Gun.CreateInstance();
        c.AddAccessory(gun);
        Assert.Single(c.Accessories);
        Assert.Equal(2, c.Effects.Count);
        Assert.Same(gun.Effects[0], c.Effects[0]);
        Assert.Same(gun.Effects[1], c.Effects[1]);

        Assert.True(c.RemoveAccessory(gun));
        Assert.Empty(c.Accessories);
        Assert.Empty(c.Effects);
    }

    [Fact]
    public void Shoot_does_not_fire_without_shoot_effect()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(
            3, 3, 1, gen, definition: TestContent.Bare);
        w.AddCharacter(2, player.Position + new SimVec2(40f, 0f));

        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetFireHeld(true);
        w.Tick(0.016f);
        Assert.Empty(w.Missiles);
    }

    [Fact]
    public void Shoot_cooldown_lives_on_effect()
    {
        var gen = new AllGrassGenerator();
        var (w, driver, player) = TestWorldHelpers.CreateDriven(3, 3, 1, gen);

        var effect = Assert.IsType<TestShootEffect>(
            Assert.Single(player.Effects.OfType<TestShootEffect>()));
        Assert.Equal(0f, effect.CooldownRemaining);

        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetFireHeld(true);
        w.Tick(0.016f);
        Assert.True(w.Missiles.Count >= 1);
        Assert.Equal(effect.FireIntervalSeconds, effect.CooldownRemaining);
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
