using Minimap.Extensive;

namespace CompuQuest.Minimap;

/// <summary>Registers CompuQuest contributions with Minimap.</summary>
public sealed class CompuQuestExtension : IExtension
{
    public void Register(IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        registry.AddIntegrator(new CompuQuestIntegrator());

        var gun = GunAccessory.CreateDefinition();
        registry.AddAccessoryDefinition(gun);
        registry.AddCharacterDefinition(GenericCharacter.CreateDefinition(gun));
    }
}
