namespace Minimap.Simulation.Types;

/// <summary>Independent accessory modifier/effect. Behavior-specific data lives on concrete effect types.</summary>
public abstract class AccessoryEffect
{
    /// <summary>Clone for a new accessory instance (runtime state starts fresh).</summary>
    public abstract AccessoryEffect Clone();
}
