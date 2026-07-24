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
    private bool _abilityActivatePressed;
    private bool _abilityBackPressed;
    private int? _modalSelect;
    private bool _placementPreview;

    public PlayerController(Player player, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        Player = player;
        _random = random ?? Random.Shared;
    }

    public Player Player { get; }

    public Character? Pawn { get; private set; }

    public bool IsPlacementPreviewing => _placementPreview;

    public HexAxial? PlacementPreviewCell { get; private set; }

    public bool PlacementPreviewValid { get; private set; }

    public void Possess(Character character) => Pawn = character;

    public void Unpossess()
    {
        Pawn = null;
        ExitPlacementPreview();
    }

    public void SetMoveInput(SimVec2 direction) => _moveInput = direction;

    public void SetAimInput(SimVec2 direction) => _aimInput = direction;

    public void SetFireHeld(bool held) => _fireHeld = held;

    public void SetAbilityActivatePressed(bool pressed) => _abilityActivatePressed = pressed;

    public void SetAbilityBackPressed(bool pressed) => _abilityBackPressed = pressed;

    public void SetModalSelect(int? slotIndex) => _modalSelect = slotIndex;

    public void Tick(GameWorld world, float dt)
    {
        if (Pawn is null || !Pawn.IsAlive)
            return;

        Pawn.MoveIntent = _moveInput;

        if (_modalSelect is int slot)
        {
            if (_placementPreview)
                ExitPlacementPreview();
            Pawn.AbilityLoadout.SelectModal(slot);
        }

        if (_abilityBackPressed && _placementPreview)
            ExitPlacementPreview();

        if (_abilityActivatePressed)
            HandleAbilityActivate(world);

        UpdatePlacementPreview(world);

        var wantsFire = _fireHeld
            && Pawn.AbilityLoadout.TryGetDedicated(AccessoryActivationBinds.PrimaryFire, out _);
        Shoot.Tick(world, Pawn, dt, _aimInput, wantsFire);
    }

    private void HandleAbilityActivate(GameWorld world)
    {
        if (Pawn is null)
            return;

        var placement = AbilityLoadout.FindPlacementEffect(Pawn.AbilityLoadout.SelectedModal);
        if (placement is null)
            return;

        if (!_placementPreview)
        {
            _placementPreview = true;
            return;
        }

        var cell = CellFacing.CellInFront(Pawn, world.HexSize);
        if (placement.TryPlace(world, Pawn, cell, _random))
            ExitPlacementPreview();
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

    private void ExitPlacementPreview()
    {
        _placementPreview = false;
        PlacementPreviewCell = null;
        PlacementPreviewValid = false;
    }
}
