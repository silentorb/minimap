using Minimap.Extensive;

namespace CompuQuest.Minimap;

/// <summary>Registers CompuQuest contributions with Minimap.</summary>
public sealed class CompuQuestExtension : IExtension
{
    public void Register(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.AddAccessoryEffectFactory(ShootEffectFactory.TypeId, ShootEffectFactory.Create);
        registry.AddIntegrator(new CompuQuestIntegrator());
    }
}
