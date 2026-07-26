using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "drain_resource_by_distance"</c>.</summary>
public static class DrainResourceByDistanceEffectFactory
{
    public const string TypeId = "drain_resource_by_distance";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!EffectJson.TryGetString(effectObject, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include id.",
                    sourcePath));
        }

        if (!registry.TryGetResourceDefinition(id, out var resource) || resource is null)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} resource '{id}' is not registered.",
                    sourcePath));
        }

        var unitsPerAmount = EffectJson.RequireFloat(
            effectObject, "unitsPerAmount", TypeId, index, sourcePath);
        if (unitsPerAmount <= 0f)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} unitsPerAmount must be > 0.",
                    sourcePath));
        }

        return new DrainResourceByDistanceEffect(resource.Tag, unitsPerAmount);
    }
}
