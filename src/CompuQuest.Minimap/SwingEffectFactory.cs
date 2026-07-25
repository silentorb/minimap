using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses accessory JSON effect <c>type: "swing"</c> into <see cref="SwingEffect"/>.</summary>
public static class SwingEffectFactory
{
    public const string TypeId = "swing";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var fireInterval = EffectJson.RequireFloat(effectObject, "fireIntervalSeconds", TypeId, index, sourcePath);
        var damage = EffectJson.RequireInt(effectObject, "damage", TypeId, index, sourcePath);
        var radius = EffectJson.RequireFloat(effectObject, "radius", TypeId, index, sourcePath);
        var arcDegrees = EffectJson.RequireFloat(effectObject, "arcDegrees", TypeId, index, sourcePath);
        var visualDuration = EffectJson.RequireFloat(
            effectObject,
            "visualDurationSeconds",
            TypeId,
            index,
            sourcePath);
        var friendlyFire = true;
        if (effectObject.TryGetProperty("friendlyFire", out var ff) &&
            (ff.ValueKind == JsonValueKind.True || ff.ValueKind == JsonValueKind.False))
        {
            friendlyFire = ff.GetBoolean();
        }

        var (costTag, costAmount) = EffectJson.ParseOptionalCost(effectObject, index, sourcePath, registry);

        try
        {
            return new SwingEffect(
                fireInterval,
                damage,
                radius,
                arcDegrees,
                visualDuration,
                friendlyFire,
                costTag,
                costAmount);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Invalid swing effect values at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }
}
