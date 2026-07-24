using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "harvest"</c>.</summary>
public static class HarvestEffectFactory
{
    public const string TypeId = "harvest";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        var (costTag, costAmount) = EffectJson.ParseOptionalCost(effectObject, index, sourcePath, registry);
        return new HarvestEffect(costTag, costAmount);
    }
}
