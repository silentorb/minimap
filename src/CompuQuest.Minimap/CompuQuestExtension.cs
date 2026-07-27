using Minimap.Extensive;

namespace CompuQuest.Minimap;

/// <summary>Registers CompuQuest contributions with Minimap.</summary>
public sealed class CompuQuestExtension : IExtension
{
    public void Register(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.RegisterTags([
            CompuQuestIntegrator.PlayerSelectableTag,
            "gardening",
            "computing",
            "medical",
            HealEffect.HumanTagName,
            HealEffect.AnimalTagName,
        ]);
        registry.AddAccessoryEffectFactory(ShootEffectFactory.TypeId, ShootEffectFactory.Create);
        registry.AddAccessoryEffectFactory(SwingEffectFactory.TypeId, SwingEffectFactory.Create);
        registry.AddAccessoryEffectFactory(MoveEffectFactory.TypeId, MoveEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            PlaceRandomActorEffectFactory.TypeId,
            PlaceRandomActorEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceEffectFactory.TypeId,
            ModifyResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(GrowEffectFactory.TypeId, GrowEffectFactory.Create);
        registry.AddAccessoryEffectFactory(SpawnEffectFactory.TypeId, SpawnEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            SpawnNearbyAllyEffectFactory.TypeId,
            SpawnNearbyAllyEffectFactory.Create);
        registry.AddAccessoryEffectFactory(HarvestEffectFactory.TypeId, HarvestEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            PickupResourceEffectFactory.TypeId,
            PickupResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            DeathDropEffectFactory.TypeId,
            DeathDropEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            UseComputerEffectFactory.TypeId,
            UseComputerEffectFactory.Create);
        registry.AddAccessoryEffectFactory(HealEffectFactory.TypeId, HealEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            DrainResourceEffectFactory.TypeId,
            DrainResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            DrainResourceByDistanceEffectFactory.TypeId,
            DrainResourceByDistanceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceByRatioBandsEffectFactory.TypeId,
            ModifyResourceByRatioBandsEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceOnUseEffectFactory.TypeId,
            ModifyResourceOnUseEffectFactory.Create);
        registry.AddIntegrator(new CompuQuestIntegrator());
    }
}
