using Minimap.Simulation.Types;

namespace Minimap.Simulation;

/// <summary>Dedicated binds and modal ability pool derived from a character's accessories.</summary>
public sealed class AbilityLoadout
{
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

        var previousSelectedId = SelectedModal?.Definition.Id;
        var previousModalIds = new List<string>(_modal.Count);
        foreach (var accessory in _modal)
            previousModalIds.Add(accessory.Definition.Id);
        var previousIndex = SelectedModalIndex;

        _dedicated.Clear();
        _modal.Clear();

        foreach (var accessory in accessories)
        {
            if (!accessory.IsEnabled)
                continue;

            var activation = accessory.Definition.Activation;
            switch (activation.Kind)
            {
                case AccessoryActivationKind.Dedicated:
                    _dedicated[activation.Bind!] = accessory;
                    break;
                case AccessoryActivationKind.Modal:
                    _modal.Add(accessory);
                    break;
            }
        }

        if (previousSelectedId is not null)
        {
            var stillIndex = IndexOfModal(previousSelectedId);
            if (stillIndex >= 0)
            {
                SelectedModalIndex = stillIndex;
                return;
            }

            for (var i = previousIndex + 1; i < previousModalIds.Count; i++)
            {
                var nextIndex = IndexOfModal(previousModalIds[i]);
                if (nextIndex >= 0)
                {
                    SelectedModalIndex = nextIndex;
                    return;
                }
            }

            SelectedModalIndex = _modal.Count > 0 ? 0 : 0;
            return;
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

    public void CycleModal(int delta)
    {
        if (_modal.Count == 0 || delta == 0)
            return;

        var count = _modal.Count;
        SelectedModalIndex = ((SelectedModalIndex + delta) % count + count) % count;
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

    public static IInteractionEffect? FindInteractionEffect(Accessory? accessory)
    {
        if (accessory is null)
            return null;
        foreach (var effect in accessory.Effects)
        {
            if (effect is IInteractionEffect interaction)
                return interaction;
        }

        return null;
    }

    public static bool TryActivateInstantUse(Accessory? accessory, Actor actor)
    {
        if (accessory is null)
            return false;

        ArgumentNullException.ThrowIfNull(actor);
        var used = false;
        foreach (var effect in accessory.Effects)
        {
            if (effect is IInstantUseEffect instant && instant.TryUse(actor))
                used = true;
        }

        return used;
    }

    private int IndexOfModal(string definitionId)
    {
        for (var i = 0; i < _modal.Count; i++)
        {
            if (string.Equals(_modal[i].Definition.Id, definitionId, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }
}
