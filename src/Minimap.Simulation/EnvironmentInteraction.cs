namespace Minimap.Simulation;

/// <summary>Resolve environment-interact targets from facing + default / ability override.</summary>
public static class EnvironmentInteraction
{
    public static Actor? ResolveTarget(GameWorld world, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);

        return Resolve(world, actor, out var target) is not null ? target : null;
    }

    public static bool TryInteract(GameWorld world, Actor actor)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);

        var interaction = Resolve(world, actor, out var target);
        if (interaction is null || target is null)
            return false;

        return interaction.TryInteract(world, actor, target);
    }

    /// <summary>
    /// Ability interaction overrides default when it can interact; otherwise default on the object.
    /// Scans front-cell candidates (free actors, then cell placeable).
    /// </summary>
    public static IInteractionEffect? Resolve(
        GameWorld world,
        Actor actor,
        out Actor? target)
    {
        target = null;
        var candidates = GetFrontCandidates(world, actor);
        if (candidates.Count == 0)
            return null;

        var ability = AbilityLoadout.FindInteractionEffect(actor.AbilityLoadout.SelectedModal);
        if (ability is not null)
        {
            foreach (var candidate in candidates)
            {
                if (!ability.CanInteract(world, actor, candidate))
                    continue;
                target = candidate;
                return ability;
            }
        }

        foreach (var candidate in candidates)
        {
            var defaultInteraction = FindDefaultInteraction(candidate);
            if (defaultInteraction is null ||
                !defaultInteraction.CanInteract(world, actor, candidate))
            {
                continue;
            }

            target = candidate;
            return new DefaultInteractionAdapter(defaultInteraction);
        }

        return null;
    }

    public static IDefaultInteractionEffect? FindDefaultInteraction(Actor target)
    {
        ArgumentNullException.ThrowIfNull(target);
        foreach (var effect in target.Effects)
        {
            if (effect is IDefaultInteractionEffect interaction)
                return interaction;
        }

        return null;
    }

    /// <summary>
    /// Front-hex candidates: living free actors on that cell (stable by id), then cell placeable.
    /// </summary>
    internal static List<Actor> GetFrontCandidates(GameWorld world, Actor actor)
    {
        var cell = CellFacing.CellInFront(actor, world.HexSize);
        var free = new List<Actor>();
        foreach (var other in world.Actors)
        {
            if (!other.IsAlive || other.IsProjectile || other.Cell is not null || other.Id == actor.Id)
                continue;
            if (HexWorldLayout.WorldToAxial(other.Position, world.HexSize) != cell)
                continue;
            free.Add(other);
        }

        free.Sort(static (a, b) => a.Id.CompareTo(b.Id));

        var candidates = new List<Actor>(free.Count + 1);
        candidates.AddRange(free);
        if (world.TryGetActorAt(cell, out var placeable) && placeable is not null)
            candidates.Add(placeable);

        return candidates;
    }

    /// <summary>Adapts <see cref="IDefaultInteractionEffect"/> to the ability interaction invoke path.</summary>
    private sealed class DefaultInteractionAdapter : IInteractionEffect
    {
        private readonly IDefaultInteractionEffect _inner;

        public DefaultInteractionAdapter(IDefaultInteractionEffect inner) => _inner = inner;

        public bool CanInteract(GameWorld world, Actor actor, Actor target) =>
            _inner.CanInteract(world, actor, target);

        public bool TryInteract(GameWorld world, Actor actor, Actor target) =>
            _inner.TryInteract(world, actor, target);
    }
}
