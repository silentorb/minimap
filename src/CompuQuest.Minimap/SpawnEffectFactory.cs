using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "spawn"</c>.</summary>
public static class SpawnEffectFactory
{
    public const string TypeId = "spawn";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var interval = EffectJson.RequireFloat(
            effectObject, "intervalSeconds", TypeId, index, sourcePath);
        if (interval <= 0f)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} intervalSeconds must be > 0.",
                    sourcePath));
        }

        var volume = EffectJson.RequireInt(effectObject, "volume", TypeId, index, sourcePath);
        if (volume < 1)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} volume must be >= 1.",
                    sourcePath));
        }

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

        try
        {
            return new SpawnEffect(interval, volume, new WeightedPool<string>(entries));
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Invalid spawn effect at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }
}
