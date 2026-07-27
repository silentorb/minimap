using System.Text.Json;
using Minimap.Extensive;
using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Parses <c>type: "move"</c>.</summary>
public static class MoveEffectFactory
{
    public const string TypeId = "move";

    public static AccessoryEffect Create(
        JsonElement effectObject,
        int index,
        string? sourcePath,
        IExtensionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        var speed = EffectJson.RequireFloat(effectObject, "speed", TypeId, index, sourcePath);
        if (speed < 0f)
        {
            throw new InvalidOperationException(
                EffectJson.AppendSource(
                    $"Accessory effect '{TypeId}' at index {index} speed must be >= 0.",
                    sourcePath));
        }

        return new MoveEffect(speed);
    }
}
