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
    /// </summary>
    public static IInteractionEffect? Resolve(
        GameWorld world,
        Actor actor,
        out Actor? target)
    {
        target = GetFrontActor(world, actor);
        if (target is null)
            return null;

        var ability = AbilityLoadout.FindInteractionEffect(actor.AbilityLoadout.SelectedModal);
        if (ability is not null && ability.CanInteract(world, actor, target))
            return ability;

        var defaultInteraction = FindDefaultInteraction(target);
        if (defaultInteraction is not null && defaultInteraction.CanInteract(world, actor, target))
            return new DefaultInteractionAdapter(defaultInteraction);

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

    private static Actor? GetFrontActor(GameWorld world, Actor actor)
    {
        var cell = CellFacing.CellInFront(actor, world.HexSize);
        if (!world.TryGetActorAt(cell, out var target) || target is null)
            return null;
        return target;
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
