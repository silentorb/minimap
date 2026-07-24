using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "modify_resource_by_ratio_bands"</c>.</summary>
public static class ModifyResourceByRatioBandsEffectFactory
{
    public const string TypeId = "modify_resource_by_ratio_bands";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var source = RequireResource(effectObject, "source", index, sourcePath, registry);
        var target = RequireResource(effectObject, "target", index, sourcePath, registry);
        var periodSeconds = EffectJson.RequireFloat(
            effectObject, "periodSeconds", TypeId, index, sourcePath);
        if (periodSeconds <= 0f)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} periodSeconds must be > 0.",
                    sourcePath));
        }

        if (!effectObject.TryGetProperty("bands", out var bandsElement) ||
            bandsElement.ValueKind != JsonValueKind.Array ||
            bandsElement.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include a non-empty bands array.",
                    sourcePath));
        }

        var bands = new List<ResourceRatioBand>();
        var bandIndex = 0;
        foreach (var bandElement in bandsElement.EnumerateArray())
        {
            if (bandElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    EffectJson.AppendSource(
                        $"Accessory effect '{TypeId}' at index {index} band {bandIndex} must be an object.",
                        sourcePath));
            }

            if (!bandElement.TryGetProperty("maxRatio", out var maxRatioElement) ||
                maxRatioElement.ValueKind != JsonValueKind.Number ||
                !maxRatioElement.TryGetSingle(out var maxRatio))
            {
                throw new InvalidOperationException(
                    EffectJson.AppendSource(
                        $"Accessory effect '{TypeId}' at index {index} band {bandIndex} must include maxRatio.",
                        sourcePath));
            }

            var amount = EffectJson.RequireInt(bandElement, "amount", TypeId, index, sourcePath);
            bands.Add(new ResourceRatioBand(maxRatio, amount));
            bandIndex++;
        }

        try
        {
            return new ModifyResourceByRatioBandsEffect(source.Tag, target.Tag, periodSeconds, bands);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(ex.Message, sourcePath), ex);
        }
    }

    private static ResourceDefinition RequireResource(
        JsonElement effectObject,
        string field,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        if (!EffectJson.TryGetString(effectObject, field, out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include {field}.",
                    sourcePath));
        }

        if (!registry.TryGetResourceDefinition(id, out var resource) || resource is null)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} {field} resource '{id}' is not registered.",
                    sourcePath));
        }

        return resource;
    }
}
