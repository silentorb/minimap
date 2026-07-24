using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Dedicated binds and modal ability pool derived from a character's accessories.</summary>
public sealed class AbilityLoadout
{
    public const int MaxModalSlots = 4;

    private readonly Dictionary<string, Accessory> _dedicated = new(StringComparer.Ordinal);
    private readonly List<Accessory> _modal = new();

    public IReadOnlyDictionary<string, Accessory> Dedicated => _dedicated;

    public IReadOnlyList<Accessory> Modal => _modal;

    public int SelectedModalIndex { get; private set; }

    public Accessory? SelectedModal =>
        _modal.Count == 0 || SelectedModalIndex < 0 || SelectedModalIndex >= _modal.Count
            ? null
            : _modal[SelectedModalIndex];

    public void Rebuild(IReadOnlyList<Accessory> accessories)
    {
        ArgumentNullException.ThrowIfNull(accessories);
        _dedicated.Clear();
        _modal.Clear();

        foreach (var accessory in accessories)
        {
            var activation = accessory.Definition.Activation;
            switch (activation.Kind)
            {
                case AccessoryActivationKind.Dedicated:
                    _dedicated[activation.Bind!] = accessory;
                    break;
                case AccessoryActivationKind.Modal:
                    if (_modal.Count < MaxModalSlots)
                        _modal.Add(accessory);
                    break;
            }
        }

        if (SelectedModalIndex >= _modal.Count)
            SelectedModalIndex = Math.Max(0, _modal.Count - 1);
    }

    public void SelectModal(int index)
    {
        if (index < 0 || index >= _modal.Count)
            return;
        SelectedModalIndex = index;
    }

    public bool TryGetDedicated(string bind, out Accessory? accessory)
    {
        if (string.IsNullOrWhiteSpace(bind))
        {
            accessory = null;
            return false;
        }

        return _dedicated.TryGetValue(bind, out accessory);
    }

    public static ICellPlacementEffect? FindPlacementEffect(Accessory? accessory)
    {
        if (accessory is null)
            return null;
        foreach (var effect in accessory.Effects)
        {
            if (effect is ICellPlacementEffect placement)
                return placement;
        }

        return null;
    }
}
