namespace Minimap.Simulation.Types;

/// <summary>Content definition for a possessable character (actor specialization).</summary>
public sealed class CharacterDefinition : ActorDefinition
{
    public CharacterDefinition(
        string id,
        IEnumerable<AccessoryDefinition> accessories,
        DepictionConfig? depictionConfig = null,
        IconConfig? iconConfig = null,
        string? displayName = null,
        IEnumerable<ActorResourceAmount>? resources = null)
        : base(id, accessories, depictionConfig, iconConfig, displayName, resources)
    {
        ArgumentNullException.ThrowIfNull(accessories);
    }
}
