using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>One local human participant in a playthrough.</summary>
public sealed class Player
{
    private readonly List<AccessoryDefinition> _selectedAccessories = new();

    public Player(int id, int accessoryPoints)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (accessoryPoints < 0)
            throw new ArgumentOutOfRangeException(nameof(accessoryPoints));

        Id = id;
        AccessoryPoints = accessoryPoints;
    }

    public int Id { get; }

    /// <summary>Starting accessory point budget for this player.</summary>
    public int AccessoryPoints { get; }

    public Character? Character { get; internal set; }

    public IReadOnlyList<AccessoryDefinition> SelectedAccessories => _selectedAccessories;

    public void SetSelectedAccessories(IEnumerable<AccessoryDefinition> accessories)
    {
        ArgumentNullException.ThrowIfNull(accessories);
        _selectedAccessories.Clear();
        _selectedAccessories.AddRange(accessories);
    }
}
