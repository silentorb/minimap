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

        var projectileActorId = EffectJson.RequireString(
            effectObject, "projectileActorId", TypeId, index, sourcePath);
        var fireInterval = EffectJson.RequireFloat(
            effectObject, "fireIntervalSeconds", TypeId, index, sourcePath);
        var missileSpeed = EffectJson.RequireFloat(
            effectObject, "missileSpeed", TypeId, index, sourcePath);
        var missileDamage = EffectJson.RequireInt(
            effectObject, "missileDamage", TypeId, index, sourcePath);
        var missileRange = EffectJson.RequireFloat(
            effectObject, "missileRange", TypeId, index, sourcePath);

        var missileSizeScale = 1f;
        if (effectObject.TryGetProperty("missileSizeScale", out var scaleEl) &&
            scaleEl.ValueKind == JsonValueKind.Number)
        {
            missileSizeScale = scaleEl.GetSingle();
        }

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
                projectileActorId,
                fireInterval,
                missileSpeed,
                missileDamage,
                missileRange,
                missileSizeScale,
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
