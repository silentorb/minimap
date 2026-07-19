namespace Minimap.Simulation.Tests;

/// <summary>Test double for human-like control (PlayerController lives in Client).</summary>
internal sealed class DriveController : IController
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

internal static class TestWorldHelpers
{
    public static (GameWorld World, DriveController Driver, Character Pawn) CreateDriven(
        int radiusX,
        int radiusY,
        int seed,
        IWorldGenerator? generator = null,
        SpawnConfig? spawn = null)
    {
        var config = spawn ?? new SpawnConfig { AiPerFaction = 0 };
        var w = GameWorld.Create(radiusX, radiusY, seed, generator, spawn: config);
        var human = FindUnpossessedHuman(w, config.PlayerFactionId);
        var driver = new DriveController();
        w.AttachController(driver, human);
        return (w, driver, human);
    }

    public static Character FindUnpossessedHuman(GameWorld world, int playerFactionId)
    {
        var controlled = new HashSet<int>();
        foreach (var c in world.Controllers)
        {
            if (c.Pawn is { } pawn)
                controlled.Add(pawn.Id);
        }

        return world.Characters.First(c => c.FactionId == playerFactionId && !controlled.Contains(c.Id));
    }
}
