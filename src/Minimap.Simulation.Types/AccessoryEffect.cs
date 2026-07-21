namespace Minimap.Simulation.Types;

/// <summary>
/// Contract base for independent accessory effects.
/// Concrete sealed effects live in content extensions (default: CompuQuest).
/// </summary>
public abstract class AccessoryEffect
{
    /// <summary>Clone for a new accessory instance (runtime state starts fresh).</summary>
    public abstract AccessoryEffect Clone();
}
