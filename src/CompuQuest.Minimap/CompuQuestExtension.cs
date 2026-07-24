using Minimap.Extensive;

namespace CompuQuest.Minimap;

/// <summary>Registers CompuQuest contributions with Minimap.</summary>
public sealed class CompuQuestExtension : IExtension
{
    public void Register(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.RegisterTags([CompuQuestIntegrator.PlayerSelectableTag]);
        registry.AddAccessoryEffectFactory(ShootEffectFactory.TypeId, ShootEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            PlaceRandomActorEffectFactory.TypeId,
            PlaceRandomActorEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceEffectFactory.TypeId,
            ModifyResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(GrowEffectFactory.TypeId, GrowEffectFactory.Create);
        registry.AddAccessoryEffectFactory(HarvestEffectFactory.TypeId, HarvestEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            DrainResourceEffectFactory.TypeId,
            DrainResourceEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceByRatioBandsEffectFactory.TypeId,
            ModifyResourceByRatioBandsEffectFactory.Create);
        registry.AddAccessoryEffectFactory(
            ModifyResourceOnUseEffectFactory.TypeId,
            ModifyResourceOnUseEffectFactory.Create);
        registry.AddIntegrator(new CompuQuestIntegrator());
    }
}
