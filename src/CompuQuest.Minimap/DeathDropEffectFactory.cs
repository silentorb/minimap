using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "death_drop"</c>. Actor id is resolved at death time (actors load after accessories).</summary>
public static class DeathDropEffectFactory
{
    public const string TypeId = "death_drop";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!EffectJson.TryGetString(effectObject, "id", out var actorId) ||
            string.IsNullOrWhiteSpace(actorId))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include id (actor definition).",
                    sourcePath));
        }

        return new DeathDropEffect(actorId);
    }
}
