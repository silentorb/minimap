using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Human-driven controller: client feeds move axes; autoshoot is shared.</summary>
public sealed class PlayerController : IController
{
    private SimVec2 _moveInput;
    private float _fireCooldown;

    public Character? Pawn { get; private set; }

    public void Possess(Character character)
    {
        Pawn = character;
        _fireCooldown = 0f;
    }

    public void Unpossess() => Pawn = null;

    public void SetMoveInput(SimVec2 direction) => _moveInput = direction;

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;
        Pawn.MoveIntent = _moveInput;
        Autoshoot.Tick(world, Pawn, ref _fireCooldown, dt);
    }
}
