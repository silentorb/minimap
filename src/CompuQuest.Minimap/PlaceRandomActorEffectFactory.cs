using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "place_random_actor"</c>.</summary>
public static class PlaceRandomActorEffectFactory
{
    public const string TypeId = "place_random_actor";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!effectObject.TryGetProperty("pool", out var poolElement) ||
            poolElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include pool array.",
                    sourcePath));
        }

        var entries = new List<WeightedEntry<string>>();
        var entryIndex = 0;
        foreach (var entryElement in poolElement.EnumerateArray())
        {
            if (entryElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    EffectJson.AppendSource(
                        $"Accessory effect '{TypeId}' at index {index} pool entry {entryIndex} must be an object.",
                        sourcePath));
            }

            if (!EffectJson.TryGetString(entryElement, "id", out var id) || string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException(
                    EffectJson.AppendSource(
                        $"Accessory effect '{TypeId}' at index {index} pool entry {entryIndex} must include id.",
                        sourcePath));
            }

            if (!entryElement.TryGetProperty("weight", out var weightElement) ||
                weightElement.ValueKind != JsonValueKind.Number ||
                !weightElement.TryGetDouble(out var weight) ||
                weight <= 0)
            {
                throw new InvalidOperationException(
                    EffectJson.AppendSource(
                        $"Accessory effect '{TypeId}' at index {index} pool entry {entryIndex} weight must be > 0.",
                        sourcePath));
            }

            entries.Add(new WeightedEntry<string>(id, weight));
            entryIndex++;
        }

        if (entries.Count == 0)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} pool must be non-empty.",
                    sourcePath));
        }

        var (costTag, costAmount) = EffectJson.ParseOptionalCost(effectObject, index, sourcePath, registry);

        try
        {
            return new PlaceRandomActorEffect(
                new WeightedPool<string>(entries),
                costTag,
                costAmount);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Invalid place_random_actor effect at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }
}
