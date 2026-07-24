using System.Text.Json;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses accessory JSON effect <c>type: "shoot"</c> into <see cref="ShootEffect"/>.</summary>
public static class ShootEffectFactory
{
    public const string TypeId = "shoot";

    public static AccessoryEffect Create(JsonElement effectObject, int index, string? sourcePath)
    {
        var fireInterval = RequireFloat(effectObject, "fireIntervalSeconds", index, sourcePath);
        var missileSpeed = RequireFloat(effectObject, "missileSpeed", index, sourcePath);
        var missileDamage = RequireInt(effectObject, "missileDamage", index, sourcePath);
        var friendlyFire = true;
        if (effectObject.TryGetProperty("friendlyFire", out var ff) &&
            (ff.ValueKind == JsonValueKind.True || ff.ValueKind == JsonValueKind.False))
        {
            friendlyFire = ff.GetBoolean();
        }

        try
        {
            return new ShootEffect(fireInterval, missileSpeed, missileDamage, friendlyFire);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Invalid shoot effect values at index {index}: {ex.Message}",
                    sourcePath),
                ex);
        }
    }

    private static float RequireFloat(
        JsonElement effectObject,
        string field,
        int index,
        string? sourcePath)
    {
        if (!effectObject.TryGetProperty(field, out var prop) ||
            prop.ValueKind != JsonValueKind.Number ||
            !prop.TryGetSingle(out var value))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include {field}.",
                    sourcePath));
        }

        return value;
    }

    private static int RequireInt(
        JsonElement effectObject,
        string field,
        int index,
        string? sourcePath)
    {
        if (!effectObject.TryGetProperty(field, out var prop) ||
            prop.ValueKind != JsonValueKind.Number ||
            !prop.TryGetInt32(out var value))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include integer {field}.",
                    sourcePath));
        }

        return value;
    }

    private static string AppendSource(string message, string? sourcePath) =>
        string.IsNullOrWhiteSpace(sourcePath) ? message : $"{message} ({sourcePath})";
}
