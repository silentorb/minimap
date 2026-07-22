using Minimap.Simulation;

namespace Minimap.Client;

/// <summary>Human-driven controller: client feeds move and aim axes; shoot uses aim direction.</summary>
public sealed class PlayerController : IController
{
    private SimVec2 _moveInput;
    private SimVec2 _aimInput;

    public PlayerController(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        Player = player;
    }

    public Player Player { get; }

    public Character? Pawn { get; private set; }

    public void Possess(Character character) => Pawn = character;

    public void Unpossess() => Pawn = null;

    public void SetMoveInput(SimVec2 direction) => _moveInput = direction;

    public void SetAimInput(SimVec2 direction) => _aimInput = direction;

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;
        Pawn.MoveIntent = _moveInput;
        Shoot.Tick(world, Pawn, dt, _aimInput);
    }
}
