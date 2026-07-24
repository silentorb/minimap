using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses accessory JSON effect <c>type: "shoot"</c> into <see cref="ShootEffect"/>.</summary>
public static class ShootEffectFactory
{
    public const string TypeId = "shoot";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var fireInterval = EffectJson.RequireFloat(effectObject, "fireIntervalSeconds", TypeId, index, sourcePath);
        var missileSpeed = EffectJson.RequireFloat(effectObject, "missileSpeed", TypeId, index, sourcePath);
        var missileDamage = EffectJson.RequireInt(effectObject, "missileDamage", TypeId, index, sourcePath);
        var friendlyFire = true;
        if (effectObject.TryGetProperty("friendlyFire", out var ff) &&
            (ff.ValueKind == JsonValueKind.True || ff.ValueKind == JsonValueKind.False))
        {
            friendlyFire = ff.GetBoolean();
        }

        var (costTag, costAmount) = EffectJson.ParseOptionalCost(effectObject, index, sourcePath, registry);

        try
        {
            return new ShootEffect(
                fireInterval,
                missileSpeed,
                missileDamage,
                friendlyFire,
                costTag,
                costAmount);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Invalid shoot effect values at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }
}
