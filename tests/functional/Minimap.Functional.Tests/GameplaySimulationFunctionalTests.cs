using Minimap.Simulation;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.Functional.Tests;

public class GameplaySimulationFunctionalTests
{
    private static readonly ActorDefinition Generic = new(
        "generic",
        [
            TestContent.Move,
            new AccessoryDefinition(
                "gun",
                [
                    new TestShootEffect(
                        CombatTuning.FireIntervalSeconds,
                        CombatTuning.MissileSpeed,
                        CombatTuning.MissileDamage),
                ]),
        ],
        resources:
        [
            new ActorResourceAmount(
                TestContent.MaxHealthResource.Tag, CombatTuning.DefaultMaxHealth),
            new ActorResourceAmount(
                TestContent.HealthResource.Tag, CombatTuning.DefaultMaxHealth),
            new ActorResourceAmount(
                TestContent.MaxEnergyResource.Tag, CombatTuning.DefaultMaxEnergy),
            new ActorResourceAmount(
                TestContent.EnergyResource.Tag, CombatTuning.DefaultMaxEnergy),
        ]);

    [Fact]
    public void Seeded_world_has_characters_on_floor_within_grid()
    {
        var w = GameWorld.Create(3, 3, 42);
        w.SpawnDefaultRoster(new SpawnConfig { AiPerFaction = 1 }, Generic, TestContent.ResourceContext);
        Assert.Equal(1 + 1 + 1, w.Actors.Count);
        foreach (var p in w.Actors)
        {
            var hex = HexWorldLayout.WorldToAxial(p.Position, w.HexSize);
            Assert.True(w.Grid.Contains(hex));
            Assert.Equal(CellType.Grass, w.Grid.Get(hex));
        }
    }

    [Fact]
    public void Holding_right_moves_player_continuously_along_plus_x()
    {
        var gen = new FixedLayoutGenerator();
        var w = GameWorld.Create(2, 2, 1, gen);
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnActorDefinition(Generic);
        w.SpawnHumanPlayers(new SpawnConfig { AiPerFaction = 0 });
        var pawn = FindUnpossessedHuman(w, 1);
        var driver = new DriveController();
        w.AttachController(driver, pawn);
        var before = pawn.Position;

        driver.SetMoveInput(new SimVec2(1f, 0f));
        const float dt = 1f / 60f;
        for (var i = 0; i < 30; i++)
            w.Tick(dt);

        var after = pawn.Position;
        Assert.True(after.X > before.X + 1f);
        Assert.Equal(before.Y, after.Y, precision: 2);
    }

    [Fact]
    public void Zero_health_quietly_removes_character()
    {
        var w = GameWorld.Create(3, 3, 1, new FixedLayoutGenerator());
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnActorDefinition(Generic);
        w.SpawnHumanPlayers(new SpawnConfig { AiPerFaction = 0 });
        var victim = w.AddActor(2, SimVec2.Zero);
        w.ApplyDamage(victim, CombatTuning.DefaultMaxHealth);
        w.Tick(0.016f);
        Assert.DoesNotContain(victim, w.Actors);
    }

    [Fact]
    public void Missile_hit_kills_hostile_and_removes_from_world()
    {
        var w = GameWorld.Create(3, 3, 1, new FixedLayoutGenerator());
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnActorDefinition(Generic);
        w.SpawnHumanPlayers(new SpawnConfig { AiPerFaction = 0 });
        var player = FindUnpossessedHuman(w, 1);
        var enemy = w.AddActor(99, player.Position + new SimVec2(5f, 0f));
        enemy.Health = CombatTuning.MissileDamage;

        var projectile = w.AddActor(player.FactionId, player.Position, TestContent.Missile);
        projectile.Projectile = new ProjectileFlight(
            new SimVec2(CombatTuning.MissileSpeed, 0f),
            CombatTuning.MissileDamage,
            friendlyFire: true,
            player.Id,
            CombatTuning.MissileSize,
            CombatTuning.MissileRange,
            player.Position);

        for (var i = 0; i < 30; i++)
            w.Tick(1f / 60f);

        Assert.DoesNotContain(enemy, w.Actors);
        Assert.Contains(player, w.Actors);
        Assert.DoesNotContain(projectile, w.Actors);
    }

    [Fact]
    public void Holding_into_east_wall_does_not_tunnel_through()
    {
        var w = GameWorld.Create(2, 2, 1, new CorridorWithEastWallGenerator());
        w.ApplyGameContent(TestContent.Content);
        w.SetSpawnActorDefinition(Generic);
        w.SpawnHumanPlayers(new SpawnConfig { AiPerFaction = 0 });
        var pawn = FindUnpossessedHuman(w, 1);
        var wallCenter = HexWorldLayout.ToWorld(new HexAxial(1, 0), w.HexSize);
        pawn.Position = new SimVec2(wallCenter.X - w.HexSize - w.PlayerRadius - 0.5f, wallCenter.Y);

        var driver = new DriveController();
        w.AttachController(driver, pawn);
        driver.SetMoveInput(new SimVec2(1f, 0f));

        for (var i = 0; i < 60; i++)
            w.Tick(1f / 60f);

        var after = pawn.Position;
        Assert.False(
            CircleHexCollision.TryCircleConvex(
                after,
                w.PlayerRadius,
                HexWorldLayout.AbsoluteHexVertices(new HexAxial(1, 0), w.HexSize),
                out _,
                out var pen) && pen > 0.05f);

        var blockedX = after.X;
        w.Tick(1f / 60f);
        Assert.Equal(blockedX, pawn.Position.X, precision: 2);
    }

    private static Actor FindUnpossessedHuman(GameWorld world, int playerFactionId)
    {
        var controlled = new HashSet<int>();
        foreach (var c in world.Controllers)
        {
            if (c.Pawn is { } pawn)
                controlled.Add(pawn.Id);
        }

        return world.Actors.First(c => c.FactionId == playerFactionId && !controlled.Contains(c.Id));
    }

    /// <summary>Local drive double (PlayerController lives in Client).</summary>
    private sealed class DriveController : IController
    {
        private SimVec2 _moveInput;
        private SimVec2 _aimInput;

        public Actor? Pawn { get; private set; }

        public void Possess(Actor character) => Pawn = character;

        public void Unpossess() => Pawn = null;

        public void SetMoveInput(SimVec2 direction) => _moveInput = direction;

        public void SetAimInput(SimVec2 direction) => _aimInput = direction;

        public void Tick(GameWorld world, float dt)
        {
            if (Pawn is null || !Pawn.IsAlive)
                return;
            Pawn.MoveIntent = _moveInput;
            Shoot.Tick(world, Pawn, dt, _aimInput, wantsFire: true);
        }
    }
}
