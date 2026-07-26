using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "spawn_nearby_ally"</c>.</summary>
public static class SpawnNearbyAllyEffectFactory
{
    public const string TypeId = "spawn_nearby_ally";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!EffectJson.TryGetString(effectObject, "characterId", out var characterId) ||
            string.IsNullOrWhiteSpace(characterId))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include characterId.",
                    sourcePath));
        }

        var aggression = AiTuning.DefaultAggression;
        if (effectObject.TryGetProperty("aggression", out var aggressionElement))
        {
            if (aggressionElement.ValueKind != JsonValueKind.Number ||
                !aggressionElement.TryGetSingle(out aggression) ||
                aggression < 0f ||
                aggression > 1f)
            {
                throw new InvalidOperationException(
                    EffectJson.AppendSource(
                        $"Accessory effect '{TypeId}' at index {index} aggression must be a number in [0, 1].",
                        sourcePath));
            }
        }

        try
        {
            return new SpawnNearbyAllyEffect(characterId, aggression);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Invalid spawn_nearby_ally effect at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }
}
