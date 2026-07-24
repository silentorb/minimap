using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

public static class PlaceRandomObjectEffectFactory
{
    public const string TypeId = "place_random_object";

    public static AccessoryEffect Create(JsonElement element, int index, string? sourcePath)
    {
        if (!element.TryGetProperty("pool", out var poolElement) || poolElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                FormatError($"effects[{index}] of type '{TypeId}' requires a pool array.", sourcePath));
        }

        var ids = new List<string>();
        var poolIndex = 0;
        foreach (var entry in poolElement.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(entry.GetString()))
            {
                throw new InvalidOperationException(
                    FormatError(
                        $"effects[{index}].pool[{poolIndex}] must be a non-empty string.",
                        sourcePath));
            }

            ids.Add(entry.GetString()!);
            poolIndex++;
        }

        if (ids.Count == 0)
        {
            throw new InvalidOperationException(
                FormatError($"effects[{index}] of type '{TypeId}' requires a non-empty pool.", sourcePath));
        }

        return new PlaceRandomObjectEffect(ids);
    }

    private static string FormatError(string message, string? sourcePath) =>
        string.IsNullOrWhiteSpace(sourcePath) ? message : $"{message} ({sourcePath})";
}
