namespace Minimap.Simulation.Types;

/// <summary>Optional enable condition: accessory is enabled when resource amount &gt;= <see cref="AtLeast"/>.</summary>
public sealed class AccessoryResourceGate
{
    public AccessoryResourceGate(TagId resourceTag, int atLeast)
    {
        if (atLeast < 0)
            throw new ArgumentOutOfRangeException(nameof(atLeast));

        ResourceTag = resourceTag;
        AtLeast = atLeast;
    }

    public TagId ResourceTag { get; }

    public int AtLeast { get; }
}
