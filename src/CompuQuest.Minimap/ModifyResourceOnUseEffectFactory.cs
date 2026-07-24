using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "modify_resource_on_use"</c>.</summary>
public static class ModifyResourceOnUseEffectFactory
{
    public const string TypeId = "modify_resource_on_use";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!EffectJson.TryGetString(effectObject, "id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} must include id.",
                    sourcePath));
        }

        if (!registry.TryGetResourceDefinition(id, out var resource) || resource is null)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} resource '{id}' is not registered.",
                    sourcePath));
        }

        var amount = EffectJson.RequireInt(effectObject, "amount", TypeId, index, sourcePath);
        var (costTag, costAmount) = EffectJson.ParseOptionalCost(effectObject, index, sourcePath, registry);
        return new ModifyResourceOnUseEffect(resource.Tag, amount, costTag, costAmount);
    }
}
