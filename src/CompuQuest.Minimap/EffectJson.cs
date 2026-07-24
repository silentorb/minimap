using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Shared JSON helpers for CompuQuest accessory effect factories.</summary>
internal static class EffectJson
{
    public static (TagId? Tag, int Amount) ParseOptionalCost(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        if (!effectObject.TryGetProperty("cost", out var costElement))
            return (null, 0);

        if (costElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} cost must be an object.",
                    sourcePath));
        }

        if (!TryGetString(costElement, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} cost must include id.",
                    sourcePath));
        }

        if (!registry.TryGetResourceDefinition(id, out var resource) || resource is null)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} cost resource '{id}' is not registered.",
                    sourcePath));
        }

        if (!costElement.TryGetProperty("amount", out var amountElement) ||
            amountElement.ValueKind != JsonValueKind.Number ||
            !amountElement.TryGetInt32(out var amount) ||
            amount < 1)
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect at index {index} cost amount must be an integer >= 1.",
                    sourcePath));
        }

        return (resource.Tag, amount);
    }

    public static float RequireFloat(
        JsonElement effectObject,
        string field,
        string typeId,
        int index,
        string? sourcePath)
    {
        if (!effectObject.TryGetProperty(field, out var prop) ||
            prop.ValueKind != JsonValueKind.Number ||
            !prop.TryGetSingle(out var value))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect '{typeId}' at index {index} must include {field}.",
                    sourcePath));
        }

        return value;
    }

    public static int RequireInt(
        JsonElement effectObject,
        string field,
        string typeId,
        int index,
        string? sourcePath)
    {
        if (!effectObject.TryGetProperty(field, out var prop) ||
            prop.ValueKind != JsonValueKind.Number ||
            !prop.TryGetInt32(out var value))
        {
            throw new InvalidOperationException(
                AppendSource(
                    $"Accessory effect '{typeId}' at index {index} must include integer {field}.",
                    sourcePath));
        }

        return value;
    }

    public static bool TryGetString(JsonElement obj, string name, out string? value)
    {
        value = null;
        if (!obj.TryGetProperty(name, out var prop) || prop.ValueKind != JsonValueKind.String)
            return false;
        value = prop.GetString();
        return true;
    }

    public static string AppendSource(string message, string? sourcePath) =>
        string.IsNullOrWhiteSpace(sourcePath) ? message : $"{message} ({sourcePath})";
}
