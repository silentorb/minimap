namespace Minimap.Simulation.Types;

/// <summary>
/// Optional activation cost on an accessory effect (default: free when not implemented or tag null).
/// </summary>
public interface IEffectUseCost
{
    /// <summary>Resource tag to consume, or null when free.</summary>
    TagId? CostResourceTag { get; }

    /// <summary>Amount to consume on successful use. Ignored when tag is null; must be &gt;= 0.</summary>
    int CostAmount { get; }
}
