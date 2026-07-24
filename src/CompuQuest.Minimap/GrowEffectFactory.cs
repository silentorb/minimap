using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "grow"</c>.</summary>
public static class GrowEffectFactory
{
    public const string TypeId = "grow";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var duration = EffectJson.RequireFloat(effectObject, "durationSeconds", TypeId, index, sourcePath);
        if (duration < 0f)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} durationSeconds must be >= 0.",
                    sourcePath));
        }

        if (!effectObject.TryGetProperty("matureDepiction", out var depictionElement) ||
            depictionElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include matureDepiction object.",
                    sourcePath));
        }

        var matureDepiction = ParseDepiction(depictionElement, index, sourcePath);

        if (!effectObject.TryGetProperty("harvestYield", out var yieldElement) ||
            yieldElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include harvestYield object.",
                    sourcePath));
        }

        if (!EffectJson.TryGetString(yieldElement, "id", out var yieldId) ||
            string.IsNullOrWhiteSpace(yieldId))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} harvestYield must include id.",
                    sourcePath));
        }

        if (!registry.TryGetResourceDefinition(yieldId, out var resource) || resource is null)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} harvestYield resource '{yieldId}' is not registered.",
                    sourcePath));
        }

        var yieldAmount = EffectJson.RequireInt(yieldElement, "amount", TypeId, index, sourcePath);
        if (yieldAmount < 0)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} harvestYield amount must be >= 0.",
                    sourcePath));
        }

        return new GrowEffect(duration, matureDepiction, resource.Tag, yieldAmount);
    }

    private static DepictionConfig ParseDepiction(JsonElement obj, int index, string? sourcePath)
    {
        if (!EffectJson.TryGetString(obj, "kind", out var kind) || string.IsNullOrWhiteSpace(kind))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} matureDepiction must include kind.",
                    sourcePath));
        }

        if (!EffectJson.TryGetString(obj, "path", out var path) || string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} matureDepiction must include path.",
                    sourcePath));
        }

        EffectJson.TryGetString(obj, "animation", out var animation);
        return new DepictionConfig(kind, path, animation);
    }
}
