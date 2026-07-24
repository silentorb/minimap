using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Afford / consume helpers for accessory-declared resources.</summary>
public static class AccessoryResources
{
    public static bool CanAffordUse(Character character, Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(accessory);

        if (accessory.Definition.ConsumedResourceTag is not { } tag)
            return true;

        return character.GetResource(tag) > 0;
    }

    public static bool TryConsumeUse(Character character, Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(accessory);

        if (accessory.Definition.ConsumedResourceTag is not { } tag)
            return true;

        return character.TryConsumeResource(tag, 1);
    }
}
