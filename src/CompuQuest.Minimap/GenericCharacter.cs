using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>Short-term generic character used for all spawns.</summary>
public static class GenericCharacter
{
    public const string DefinitionId = "generic";

    public static CharacterDefinition CreateDefinition(AccessoryDefinition gun) =>
        new(DefinitionId, [gun]);
}
