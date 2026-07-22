using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>
/// Pure accessory selection state for one lobby slot.
/// Choices persist Claimed↔Ready; cleared when returning to Available.
/// Cursor/focus is UI-only and is not stored here.
/// </summary>
public sealed class LobbyAccessorySelectionState
{
    private readonly List<AccessoryDefinition> _owned = new();

    public LobbyAccessorySelectionState(int accessoryPoints)
    {
        if (accessoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(accessoryPoints));
        AccessoryPoints = accessoryPoints;
        RemainingPoints = accessoryPoints;
    }

    public int AccessoryPoints { get; }

    public int RemainingPoints { get; private set; }

    public IReadOnlyList<AccessoryDefinition> Owned => _owned;

    public void Reset()
    {
        _owned.Clear();
        RemainingPoints = AccessoryPoints;
    }

    public bool TryTake(AccessoryDefinition accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (_owned.Contains(accessory))
            return false;
        if (accessory.PointCost > RemainingPoints)
            return false;

        _owned.Add(accessory);
        RemainingPoints -= accessory.PointCost;
        return true;
    }

    public bool TryReturn(AccessoryDefinition accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (!_owned.Remove(accessory))
            return false;

        RemainingPoints += accessory.PointCost;
        return true;
    }
}
