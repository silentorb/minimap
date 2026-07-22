using Minimap.Simulation.Types;

namespace Minimap.Client.Lobby;

/// <summary>
/// Pure accessory selection state for one lobby slot.
/// Choices persist Claimed↔Ready; cleared when returning to Available.
/// Cursor/focus is UI-only and is not stored here.
/// </summary>
public sealed class LobbyAccessorySelectionState
{
    private readonly List<AccessoryDefinition> _priorOwned = new();
    private readonly List<AccessoryDefinition> _stageOwned = new();
    private readonly List<AccessoryDefinition> _ownedView = new();

    /// <param name="accessoryPoints">Point budget for this selection stage.</param>
    /// <param name="priorOwned">
    /// Accessories already acquired in earlier stages. Shown in Owned but cannot be returned.
    /// </param>
    public LobbyAccessorySelectionState(
        int accessoryPoints,
        IEnumerable<AccessoryDefinition>? priorOwned = null)
    {
        if (accessoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(accessoryPoints));

        AccessoryPoints = accessoryPoints;
        RemainingPoints = accessoryPoints;

        if (priorOwned is not null)
        {
            foreach (var accessory in priorOwned)
            {
                ArgumentNullException.ThrowIfNull(accessory);
                if (_priorOwned.Any(a => a.Id == accessory.Id))
                    continue;
                _priorOwned.Add(accessory);
            }
        }

        RebuildOwnedView();
    }

    public int AccessoryPoints { get; }

    /// <summary>Points still available to spend this stage.</summary>
    public int RemainingPoints { get; private set; }

    /// <summary>All owned accessories (prior-stage locked first, then this-stage choices).</summary>
    public IReadOnlyList<AccessoryDefinition> Owned => _ownedView;

    public IReadOnlyList<AccessoryDefinition> PriorOwned => _priorOwned;

    public IReadOnlyList<AccessoryDefinition> StageOwned => _stageOwned;

    public bool IsLocked(AccessoryDefinition accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        return _priorOwned.Any(a => a.Id == accessory.Id);
    }

    public bool IsOwned(AccessoryDefinition accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        return _ownedView.Any(a => a.Id == accessory.Id);
    }

    /// <summary>Clear this-stage choices only; prior-owned remain locked.</summary>
    public void ResetStage()
    {
        _stageOwned.Clear();
        RemainingPoints = AccessoryPoints;
        RebuildOwnedView();
    }

    public bool TryTake(AccessoryDefinition accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (IsOwned(accessory))
            return false;
        if (accessory.PointCost > RemainingPoints)
            return false;

        _stageOwned.Add(accessory);
        RemainingPoints -= accessory.PointCost;
        RebuildOwnedView();
        return true;
    }

    /// <summary>
    /// Return a this-stage choice to the available list. Prior-stage accessories cannot be returned.
    /// </summary>
    public bool TryReturn(AccessoryDefinition accessory)
    {
        ArgumentNullException.ThrowIfNull(accessory);
        if (IsLocked(accessory))
            return false;

        var index = _stageOwned.FindIndex(a => a.Id == accessory.Id);
        if (index < 0)
            return false;

        var removed = _stageOwned[index];
        _stageOwned.RemoveAt(index);
        RemainingPoints += removed.PointCost;
        RebuildOwnedView();
        return true;
    }

    private void RebuildOwnedView()
    {
        _ownedView.Clear();
        _ownedView.AddRange(_priorOwned);
        _ownedView.AddRange(_stageOwned);
    }
}
