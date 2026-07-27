using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Simulation.Tests;

public class HungerTests
{
    private static readonly ResourceRatioBandProxy[] VitalityBands =
    [
        new(0f, -2),
        new(1f / 3f, -1),
        new(2f / 3f, 0),
        new(1f, 1),
    ];

    [Fact]
    public void Character_starts_with_default_energy()
    {
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        Assert.Equal(CombatTuning.DefaultMaxEnergy, character.Energy);
        Assert.Equal(CombatTuning.DefaultMaxEnergy, character.MaxEnergy);
    }

    [Fact]
    public void Energy_clamps_to_max_energy()
    {
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.Energy = 999;
        Assert.Equal(character.MaxEnergy, character.Energy);
    }

    [Fact]
    public void Zero_energy_does_not_kill_character()
    {
        var world = GameWorld.Create(2, 1);
        world.ApplyGameContent(TestContent.Content);
        var character = world.AddActor(1, SimVec2.Zero, TestContent.Bare);
        character.Energy = 0;
        world.Tick(0.016f);
        Assert.Contains(character, world.Actors);
        Assert.True(character.IsAlive);
        Assert.Equal(0, character.Energy);
    }

    [Fact]
    public void Passive_drain_reduces_energy_over_time()
    {
        var world = GameWorld.Create(2, 1);
        world.ApplyGameContent(TestContent.Content);
        var pawn = world.AddActor(1, SimVec2.Zero, TestContent.Bare);
        pawn.AddAccessory(new AccessoryDefinition(
            "drain",
            [new TestDrainResourceEffect(TestContent.EnergyResource.Tag, 1f)],
            activation: AccessoryActivation.None).CreateInstance());

        var before = pawn.Energy;
        world.TickActorPassives(1f);
        Assert.Equal(before - 1, pawn.Energy);
    }

    [Theory]
    [InlineData(0, 100, -2)]
    [InlineData(1, 100, -1)]
    [InlineData(33, 100, -1)]
    [InlineData(34, 100, 0)]
    [InlineData(66, 100, 0)]
    [InlineData(67, 100, 1)]
    [InlineData(100, 100, 1)]
    public void Vitality_band_amount_matches_energy_ratio(int energy, int max, int expected)
    {
        Assert.Equal(expected, ResolveVitalityAmount(energy, max));
    }

    [Fact]
    public void Vitality_pulse_modifies_health_from_energy_band()
    {
        var world = GameWorld.Create(2, 1);
        world.ApplyGameContent(TestContent.Content);
        var pawn = world.AddActor(1, SimVec2.Zero, TestContent.Bare);
        pawn.Energy = 0;
        pawn.Health = 50;
        pawn.AddAccessory(new AccessoryDefinition(
            "vitality",
            [new TestVitalityEffect(TestContent.EnergyResource.Tag, TestContent.HealthResource.Tag, 1f)],
            activation: AccessoryActivation.None).CreateInstance());

        world.TickActorPassives(1f);
        Assert.Equal(48, pawn.Health);
    }

    [Fact]
    public void Eat_disabled_without_food_and_enabled_with_food()
    {
        var eat = CreateEatAccessoryDefinition();
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.AddAccessory(eat.CreateInstance());
        var eatInstance = Assert.Single(character.Accessories, a => a.Definition.Id == "eat");

        Assert.Empty(character.AbilityLoadout.Modal);
        Assert.False(eatInstance.IsEnabled);

        character.AddResource(TestContent.FoodResource.Tag, 1);
        Assert.True(eatInstance.IsEnabled);
        Assert.Single(character.AbilityLoadout.Modal);
        Assert.Equal("eat", character.AbilityLoadout.Modal[0].Definition.Id);
    }

    [Fact]
    public void Disabled_eat_removes_from_loadout_and_selects_next_modal()
    {
        var eat = CreateEatAccessoryDefinition();
        var farm = new AccessoryDefinition(
            "farm",
            Array.Empty<AccessoryEffect>(),
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));

        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.AddResource(TestContent.FoodResource.Tag, 1);
        character.AddAccessory(eat.CreateInstance());
        character.AddAccessory(farm.CreateInstance());
        character.AbilityLoadout.SelectModal(0);
        Assert.Equal("eat", character.AbilityLoadout.SelectedModal!.Definition.Id);

        character.TryConsumeResource(TestContent.FoodResource.Tag, 1);
        Assert.Single(character.AbilityLoadout.Modal);
        Assert.Equal("farm", character.AbilityLoadout.SelectedModal!.Definition.Id);
    }

    [Fact]
    public void Gun_with_zero_ammo_stays_in_loadout()
    {
        var gun = new AccessoryDefinition(
            "gun",
            [
                new TestGrantResourceEffect(TestContent.AmmoResource.Tag, 6),
                new TestShootEffect(1.25f, 200f, 25, true, TestContent.AmmoResource.Tag, 1),
            ],
            activation: new AccessoryActivation(
                AccessoryActivationKind.Dedicated,
                AccessoryActivationBinds.PrimaryFire));
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.AddAccessory(gun.CreateInstance());
        Assert.Equal(6, character.GetResource(TestContent.AmmoResource.Tag));
        character.TryConsumeResource(TestContent.AmmoResource.Tag, 6);
        Assert.Equal(0, character.GetResource(TestContent.AmmoResource.Tag));
        Assert.True(character.AbilityLoadout.TryGetDedicated(
            AccessoryActivationBinds.PrimaryFire, out var dedicated));
        Assert.Equal("gun", dedicated!.Definition.Id);
    }

    [Fact]
    public void Instant_use_consumes_food_and_restores_energy()
    {
        var eat = CreateEatAccessoryDefinition();
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.Energy = 50;
        character.AddResource(TestContent.FoodResource.Tag, 2);
        character.AddAccessory(eat.CreateInstance());

        Assert.True(AbilityLoadout.TryActivateInstantUse(character.AbilityLoadout.SelectedModal, character));
        Assert.Equal(55, character.Energy);
        Assert.Equal(1, character.GetResource(TestContent.FoodResource.Tag));
    }

    [Fact]
    public void Instant_use_refuses_when_cannot_afford_cost()
    {
        var eat = new AccessoryDefinition(
            "eat",
            [new TestInstantModifyResourceEffect(
                TestContent.EnergyResource.Tag,
                5,
                TestContent.FoodResource.Tag,
                1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal));
        // Force-enabled without food by omitting enabledWhen
        var character = new Actor(0, TestContent.Bare, TestContent.ResourceContext, 1, SimVec2.Zero);
        character.AddAccessory(eat.CreateInstance());
        Assert.False(AbilityLoadout.TryActivateInstantUse(character.AbilityLoadout.SelectedModal, character));
        Assert.Equal(CombatTuning.DefaultMaxEnergy, character.Energy);
    }

    private static AccessoryDefinition CreateEatAccessoryDefinition() =>
        new(
            "eat",
            [new TestInstantModifyResourceEffect(
                TestContent.EnergyResource.Tag,
                5,
                TestContent.FoodResource.Tag,
                1)],
            activation: new AccessoryActivation(AccessoryActivationKind.Modal),
            enabledWhen: new AccessoryResourceGate(TestContent.FoodResource.Tag, 1));

    private static int ResolveVitalityAmount(int energy, int max)
    {
        var ratio = max <= 0 ? 0f : energy / (float)max;
        foreach (var band in VitalityBands)
        {
            if (ratio <= band.MaxRatio)
                return band.Amount;
        }

        return VitalityBands[^1].Amount;
    }

    private readonly record struct ResourceRatioBandProxy(float MaxRatio, int Amount);
}

internal sealed class TestDrainResourceEffect : AccessoryEffect, IPassiveEffect
{
    private float _accumulator;

    public TestDrainResourceEffect(TagId resourceTag, float amountPerSecond)
    {
        ResourceTag = resourceTag;
        AmountPerSecond = amountPerSecond;
    }

    public TagId ResourceTag { get; }
    public float AmountPerSecond { get; }

    public void Tick(Actor actor, float dt)
    {
        _accumulator += AmountPerSecond * dt;
        var whole = (int)_accumulator;
        if (whole <= 0)
            return;
        _accumulator -= whole;
        actor.AddResource(ResourceTag, -whole);
    }

    public override AccessoryEffect Clone() => new TestDrainResourceEffect(ResourceTag, AmountPerSecond);
}

internal sealed class TestVitalityEffect : AccessoryEffect, IPassiveEffect
{
    private float _elapsed;

    public TestVitalityEffect(TagId sourceTag, TagId targetTag, float periodSeconds)
    {
        SourceTag = sourceTag;
        TargetTag = targetTag;
        PeriodSeconds = periodSeconds;
    }

    public TagId SourceTag { get; }
    public TagId TargetTag { get; }
    public float PeriodSeconds { get; }

    public void Tick(Actor actor, float dt)
    {
        _elapsed += dt;
        while (_elapsed >= PeriodSeconds)
        {
            _elapsed -= PeriodSeconds;
            var max = 0;
            if (actor.ResourceContext.TryGet(SourceTag, out var def) && def?.LimitTag is { } limit)
                max = actor.GetResource(limit);
            var ratio = max <= 0 ? 0f : actor.GetResource(SourceTag) / (float)max;
            var amount = ratio <= 0f ? -2
                : ratio <= 1f / 3f ? -1
                : ratio <= 2f / 3f ? 0
                : 1;
            if (amount != 0)
                actor.AddResource(TargetTag, amount);
        }
    }

    public override AccessoryEffect Clone() => new TestVitalityEffect(SourceTag, TargetTag, PeriodSeconds);
}

internal sealed class TestInstantModifyResourceEffect : AccessoryEffect, IInstantUseEffect, IEffectUseCost
{
    public TestInstantModifyResourceEffect(
        TagId resourceTag,
        int amount,
        TagId? costResourceTag = null,
        int costAmount = 0)
    {
        ResourceTag = resourceTag;
        Amount = amount;
        CostResourceTag = costResourceTag;
        CostAmount = costAmount;
    }

    public TagId ResourceTag { get; }
    public int Amount { get; }
    public TagId? CostResourceTag { get; }
    public int CostAmount { get; }

    public bool TryUse(Actor actor)
    {
        if (!EffectUseCosts.TryConsume(actor, this))
            return false;
        actor.AddResource(ResourceTag, Amount);
        return true;
    }

    public override AccessoryEffect Clone() =>
        new TestInstantModifyResourceEffect(ResourceTag, Amount, CostResourceTag, CostAmount);
}
