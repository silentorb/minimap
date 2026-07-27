using Minimap.Simulation;
using Minimap.Simulation.Types;

namespace Minimap.Client;

/// <summary>Human-driven controller: client feeds move/aim/fire and modal ability intents.</summary>
public sealed class PlayerController : IController
{
    private readonly Random _random;
    private SimVec2 _moveInput;
    private SimVec2 _aimInput;
    private bool _fireHeld;
    private bool _secondaryFireHeld;
    private bool _abilityActivatePressed;
    private bool _abilityBackPressed;
    private bool _interactPressed;
    private int? _modalCycle;
    private bool _placementPreview;

    public PlayerController(Player player, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        Player = player;
        _random = random ?? Random.Shared;
    }

    public Player Player { get; }

    public Actor? Pawn { get; private set; }

    public bool IsPlacementPreviewing => _placementPreview;

    public HexAxial? PlacementPreviewCell { get; private set; }

    public bool PlacementPreviewValid { get; private set; }

    /// <summary>Cell-actor id currently valid for environment interact, if any.</summary>
    public int? InteractTargetActorId { get; private set; }

    public void Possess(Actor character) => Pawn = character;

    public void Unpossess()
    {
        Pawn = null;
        ExitPlacementPreview();
        InteractTargetActorId = null;
    }

    public void SetMoveInput(SimVec2 direction) => _moveInput = direction;

    public void SetAimInput(SimVec2 direction) => _aimInput = direction;

    public void SetFireHeld(bool held) => _fireHeld = held;

    public void SetSecondaryFireHeld(bool held) => _secondaryFireHeld = held;

    public void SetAbilityActivatePressed(bool pressed) => _abilityActivatePressed = pressed;

    public void SetAbilityBackPressed(bool pressed) => _abilityBackPressed = pressed;

    public void SetInteractPressed(bool pressed) => _interactPressed = pressed;

    public void SetModalCycle(int? delta) => _modalCycle = delta;

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;

        Pawn.MoveIntent = _moveInput;

        if (_modalCycle is int delta)
        {
            if (_placementPreview)
                ExitPlacementPreview();
            Pawn.AbilityLoadout.CycleModal(delta);
        }

        if (_abilityBackPressed && _placementPreview)
            ExitPlacementPreview();

        if (_abilityActivatePressed)
            HandleAbilityActivate(world);

        if (_interactPressed)
            EnvironmentInteraction.TryInteract(world, Pawn);

        UpdatePlacementPreview(world);
        UpdateInteractTarget(world);

        var wantsFire = _fireHeld
            && Pawn.AbilityLoadout.TryGetDedicated(AccessoryActivationBinds.PrimaryFire, out _);
        Shoot.Tick(world, Pawn, dt, _aimInput, wantsFire);

        var wantsSwing = _secondaryFireHeld
            && Pawn.AbilityLoadout.TryGetDedicated(AccessoryActivationBinds.SecondaryFire, out _);
        Swing.Tick(world, Pawn, dt, _aimInput, wantsSwing);
    }

    private void HandleAbilityActivate(GameWorld world)
    {
        if (Pawn is null)
            return;

        var selected = Pawn.AbilityLoadout.SelectedModal;
        var placement = AbilityLoadout.FindPlacementEffect(selected);
        if (placement is not null)
        {
            if (!_placementPreview)
            {
                _placementPreview = true;
                return;
            }

            var cell = CellFacing.CellInFront(Pawn, world.HexSize);
            if (placement.TryPlace(world, Pawn, cell, _random))
                ExitPlacementPreview();
            return;
        }

        AbilityLoadout.TryActivateInstantUse(selected, Pawn);
    }

    private void UpdatePlacementPreview(GameWorld world)
    {
        PlacementPreviewCell = null;
        PlacementPreviewValid = false;
        if (!_placementPreview || Pawn is null)
            return;

        var placement = AbilityLoadout.FindPlacementEffect(Pawn.AbilityLoadout.SelectedModal);
        if (placement is null)
        {
            ExitPlacementPreview();
            return;
        }

        var cell = CellFacing.CellInFront(Pawn, world.HexSize);
        PlacementPreviewCell = cell;
        PlacementPreviewValid = placement.CanPlace(world, Pawn, cell);
    }

    private void UpdateInteractTarget(GameWorld world)
    {
        InteractTargetActorId = null;
        if (Pawn is null)
            return;

        var target = EnvironmentInteraction.ResolveTarget(world, Pawn);
        if (target is not null)
            InteractTargetActorId = target.Id;
    }

    private void ExitPlacementPreview()
    {
        _placementPreview = false;
        PlacementPreviewCell = null;
        PlacementPreviewValid = false;
    }
}
