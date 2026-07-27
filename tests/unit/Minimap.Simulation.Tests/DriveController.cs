using Minimap.Simulation.Types;

namespace Minimap.Simulation.Tests;

/// <summary>Test double for human-like control (PlayerController lives in Client).</summary>
internal sealed class DriveController : IController
{
    private SimVec2 _moveInput;
    private SimVec2 _aimInput;
    private bool _fireHeld;
    private bool _secondaryFireHeld;

    public Actor? Pawn { get; private set; }

    public void Possess(Actor character) => Pawn = character;

    public void Unpossess() => Pawn = null;

    public void SetMoveInput(SimVec2 direction) => _moveInput = direction;

    public void SetAimInput(SimVec2 direction) => _aimInput = direction;

    public void SetFireHeld(bool held) => _fireHeld = held;

    public void SetSecondaryFireHeld(bool held) => _secondaryFireHeld = held;

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;
        Pawn.MoveIntent = _moveInput;
        Shoot.Tick(world, Pawn, dt, _aimInput, _fireHeld);
        Swing.Tick(world, Pawn, dt, _aimInput, _secondaryFireHeld);
    }
}

internal static class TestWorldHelpers
{
    public static (GameWorld World, DriveController Driver, Actor Pawn) CreateDriven(
        int radiusX,
        int radiusY,
        int seed,
        IWorldGenerator? generator = null,
        SpawnConfig? spawn = null,
        ActorDefinition? definition = null)
    {
        var config = spawn ?? new SpawnConfig { AiPerFaction = 0 };
        var def = definition ?? TestContent.Generic;
        var w = GameWorld.Create(radiusX, radiusY, seed, generator);
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnActorDefinition(def);
        w.SpawnHumanPlayers(config);
        var human = FindUnpossessedHuman(w, config.PlayerFactionId);
        var driver = new DriveController();
        w.AttachController(driver, human);
        return (w, driver, human);
    }

    public static Actor FindUnpossessedHuman(GameWorld world, int playerFactionId)
    {
        var controlled = new HashSet<int>();
        foreach (var c in world.Controllers)
        {
            if (c.Pawn is { } pawn)
                controlled.Add(pawn.Id);
        }

        return world.Actors.First(c => c.FactionId == playerFactionId && !controlled.Contains(c.Id));
    }
}
