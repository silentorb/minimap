namespace Minimap.Simulation;

/// <summary>Ability effect that can target another actor for environment interaction.</summary>
public interface IInteractionEffect
{
    bool CanInteract(GameWorld world, Actor actor, Actor target);

    /// <summary>Returns false for expected rejection; does not throw for those cases.</summary>
    bool TryInteract(GameWorld world, Actor actor, Actor target);
}
