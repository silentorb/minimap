namespace Minimap.Simulation;

/// <summary>Object-side default environment interaction (used when no ability override applies).</summary>
public interface IDefaultInteractionEffect
{
    bool CanInteract(GameWorld world, Actor actor, Actor target);

    /// <summary>Returns false for expected rejection; does not throw for those cases.</summary>
    bool TryInteract(GameWorld world, Actor actor, Actor target);
}
