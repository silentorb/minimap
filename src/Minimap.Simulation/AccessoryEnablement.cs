using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Evaluates <see cref="AccessoryDefinition.EnabledWhen"/> against an actor's resources.</summary>
public static class AccessoryEnablement
{
    public static bool Evaluate(Actor actor, Accessory accessory)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(accessory);

        var gate = accessory.Definition.EnabledWhen;
        if (gate is null)
            return true;

        return actor.GetResource(gate.ResourceTag) >= gate.AtLeast;
    }

    /// <summary>
    /// Updates <see cref="Accessory.IsEnabled"/> for all accessories.
    /// Returns true when any enable flag changed.
    /// </summary>
    public static bool Sync(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var changed = false;
        foreach (var accessory in actor.Accessories)
        {
            var enabled = Evaluate(actor, accessory);
            if (accessory.IsEnabled == enabled)
                continue;
            accessory.IsEnabled = enabled;
            changed = true;
        }

        return changed;
    }
}
