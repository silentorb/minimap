using Minimap.Client;
using Minimap.Simulation;
using Minimap.Simulation.Types;
using Xunit;

namespace Minimap.App.Tests;

public class PlayerControllerFacingTests
{
    [Fact]
    public void Tick_sets_facing_from_aim_independent_of_move()
    {
        var (world, pawn) = CreatePawn();
        pawn.Facing = new SimVec2(1f, 0f);

        var controller = new PlayerController(new Player(0, accessoryPoints: 0));
        controller.Possess(pawn);
        controller.SetMoveInput(new SimVec2(0f, 1f));
        controller.SetAimInput(new SimVec2(0f, -1f));

        controller.Tick(world, 0.016f);

        Assert.Equal(0f, pawn.Facing.X, precision: 4);
        Assert.Equal(-1f, pawn.Facing.Y, precision: 4);
        Assert.Equal(0f, pawn.MoveIntent.X, precision: 4);
        Assert.Equal(1f, pawn.MoveIntent.Y, precision: 4);
    }

    [Fact]
    public void Tick_does_not_change_facing_when_aim_is_zero()
    {
        var (world, pawn) = CreatePawn();
        pawn.Facing = new SimVec2(0f, 1f);

        var controller = new PlayerController(new Player(0, accessoryPoints: 0));
        controller.Possess(pawn);
        controller.SetMoveInput(new SimVec2(1f, 0f));
        controller.SetAimInput(SimVec2.Zero);

        controller.Tick(world, 0.016f);

        Assert.Equal(0f, pawn.Facing.X, precision: 4);
        Assert.Equal(1f, pawn.Facing.Y, precision: 4);
    }

    private static (GameWorld World, Actor Pawn) CreatePawn()
    {
        var tags = new TagRegistry();
        var maxHealth = new ResourceDefinition(
            WellKnownResourceIds.MaxHealth, tags.GetOrCreate(WellKnownResourceIds.MaxHealth), visible: false);
        var health = new ResourceDefinition(
            WellKnownResourceIds.Health, tags.GetOrCreate(WellKnownResourceIds.Health),
            limitTag: maxHealth.Tag);
        var maxEnergy = new ResourceDefinition(
            WellKnownResourceIds.MaxEnergy, tags.GetOrCreate(WellKnownResourceIds.MaxEnergy), visible: false);
        var energy = new ResourceDefinition(
            WellKnownResourceIds.Energy, tags.GetOrCreate(WellKnownResourceIds.Energy),
            limitTag: maxEnergy.Tag);
        var def = new ActorDefinition(
            "pawn",
            resources:
            [
                new ActorResourceAmount(maxHealth.Tag, 100),
                new ActorResourceAmount(health.Tag, 100),
            ]);
        var world = GameWorld.Create(3, 3, 1, new AllGrassGenerator());
        world.ApplyGameContent(new GameContent(
            def,
            actors: [def],
            resources: [health, maxHealth, energy, maxEnergy]));
        var pawn = world.AddActor(1, SimVec2.Zero, def);
        return (world, pawn);
    }

    private sealed class AllGrassGenerator : IWorldGenerator
    {
        public void GenerateTerrain(HexGrid grid, Random random)
        {
            foreach (var h in grid.AllHexes())
                grid.Set(h, CellType.Grass);
        }
    }
}
