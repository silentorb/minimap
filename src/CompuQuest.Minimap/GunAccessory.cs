using Minimap.Simulation.Types;

namespace CompuQuest.Minimap;

/// <summary>CompuQuest Gun accessory (autoshoot).</summary>
public static class GunAccessory
{
    public const string DefinitionId = "gun";

    public static AccessoryDefinition CreateDefinition() =>
        new(
            DefinitionId,
            [
                new AutoshootEffect(
                    fireIntervalSeconds: 1.25f,
                    missileSpeed: 200f,
                    missileDamage: 25f),
            ]);
}
