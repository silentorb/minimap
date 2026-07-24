namespace Minimap.Simulation;

/// <summary>Resolve environment-interact targets from facing + selected ability.</summary>
public static class EnvironmentInteraction
{
    public static Actor? ResolveTarget(GameWorld world, Character actor)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);

        var cell = CellFacing.CellInFront(actor, world.HexSize);
        if (!world.TryGetActorAt(cell, out var target) || target is null)
            return null;

        var interaction = AbilityLoadout.FindInteractionEffect(actor.AbilityLoadout.SelectedModal);
        if (interaction is null || !interaction.CanInteract(world, actor, target))
            return null;

        return target;
    }

    public static bool TryInteract(GameWorld world, Character actor)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(actor);

        var target = ResolveTarget(world, actor);
        if (target is null)
            return false;

        var interaction = AbilityLoadout.FindInteractionEffect(actor.AbilityLoadout.SelectedModal);
        return interaction is not null && interaction.TryInteract(world, actor, target);
    }
}
