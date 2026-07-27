using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class CharacterResourceTests
{
    [Fact]
    public void AddAccessory_grants_starting_resource_amount()
    {
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        Assert.Equal(0, character.GetResource(TestContent.AmmoResource.Tag));

        character.AddAccessory(TestContent.Gun.CreateInstance());
        Assert.Equal(6, character.GetResource(TestContent.AmmoResource.Tag));
    }

    [Fact]
    public void TryConsumeResource_fails_when_stock_insufficient()
    {
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.SetResource(TestContent.AmmoResource.Tag, 1);

        Assert.True(character.TryConsumeResource(TestContent.AmmoResource.Tag, 1));
        Assert.False(character.TryConsumeResource(TestContent.AmmoResource.Tag, 1));
        Assert.Equal(0, character.GetResource(TestContent.AmmoResource.Tag));
    }

    [Fact]
    public void Health_clamps_to_max_health_resource()
    {
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.SetResource(TestContent.MaxHealthResource.Tag, 50);
        character.Health = 80;
        Assert.Equal(50, character.Health);
    }

    [Fact]
    public void Shoot_consumes_ammo_and_stops_at_zero()
    {
        var (w, driver, pawn) = TestWorldHelpers.CreateDriven(3, 3, 9);
        Assert.Equal(6, pawn.GetResource(TestContent.AmmoResource.Tag));

        var effect = Shoot.FindShootEffect(pawn)!;
        effect.CooldownRemaining = 0f;
        driver.SetAimInput(new SimVec2(1f, 0f));
        driver.SetFireHeld(true);

        for (var i = 0; i < 6; i++)
        {
            effect.CooldownRemaining = 0f;
            w.Tick(0.016f);
        }

        Assert.Equal(0, pawn.GetResource(TestContent.AmmoResource.Tag));
        var shootEffect = Shoot.FindShootEffectInstance(pawn)!;
        Assert.False(EffectUseCosts.CanAfford(pawn, shootEffect));

        effect.CooldownRemaining = 0f;
        w.Tick(0.016f);
        Assert.Equal(0, pawn.GetResource(TestContent.AmmoResource.Tag));
    }
}
