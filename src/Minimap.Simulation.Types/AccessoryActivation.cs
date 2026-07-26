namespace Minimap.Simulation.Types;

public enum AccessoryActivationKind : byte
{
    /// <summary>Not player-activatable (passive / equipment without a bind).</summary>
    None = 0,
    /// <summary>Fixed to a named input bind (e.g. primary fire).</summary>
    Dedicated = 1,
    /// <summary>Selectable in the modal ability pool (D-pad Left/Right / bracket keys cycle).</summary>
    Modal = 2,
}

/// <summary>Known dedicated activation bind ids.</summary>
public static class AccessoryActivationBinds
{
    public const string PrimaryFire = "primary_fire";
    public const string SecondaryFire = "secondary_fire";
}

/// <summary>How a player activates an accessory in-world.</summary>
public sealed class AccessoryActivation
{
    public static AccessoryActivation None { get; } = new(AccessoryActivationKind.None, null);

    public AccessoryActivation(AccessoryActivationKind kind, string? bind = null)
    {
        if (kind == AccessoryActivationKind.Dedicated)
        {
            if (string.IsNullOrWhiteSpace(bind))
                throw new ArgumentException("Dedicated activation requires a bind id.", nameof(bind));
        }
        else if (!string.IsNullOrWhiteSpace(bind))
        {
            throw new ArgumentException("Only dedicated activation may specify a bind.", nameof(bind));
        }

        Kind = kind;
        Bind = bind;
    }

    public AccessoryActivationKind Kind { get; }

    public string? Bind { get; }
}
