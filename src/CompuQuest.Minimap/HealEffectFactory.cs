using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "heal"</c>.</summary>
public static class HealEffectFactory
{
    public const string TypeId = "heal";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var (costTag, costAmount) = EffectJson.ParseOptionalCost(effectObject, index, sourcePath, registry);
        var humanTag = registry.Tags.GetOrCreate(HealEffect.HumanTagName);
        var animalTag = registry.Tags.GetOrCreate(HealEffect.AnimalTagName);
        return new HealEffect(humanTag, animalTag, costTag, costAmount);
    }
}
