using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "pickup_resource"</c>.</summary>
public static class PickupResourceEffectFactory
{
    public const string TypeId = "pickup_resource";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!EffectJson.TryGetString(effectObject, "id", out var resourceId) ||
            string.IsNullOrWhiteSpace(resourceId))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include id.",
                    sourcePath));
        }

        if (!registry.TryGetResourceDefinition(resourceId, out var resource) || resource is null)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} resource '{resourceId}' is not registered.",
                    sourcePath));
        }

        var amount = EffectJson.RequireInt(effectObject, "amount", TypeId, index, sourcePath);
        if (amount <= 0)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} amount must be >= 1.",
                    sourcePath));
        }

        var (costTag, costAmount) = EffectJson.ParseOptionalCost(
            effectObject, index, sourcePath, registry);
        return new PickupResourceEffect(resource.Tag, amount, costTag, costAmount);
    }
}
