# Actors (technical)

Actor definition vs instance contracts. Implements [actors.md](../../../game/features/gameplay/actors.md). Related: [characters-and-factions.md](characters-and-factions.md), [accessories.md](accessories.md), [health.md](../../../game/features/gameplay/health.md), [cell-placement.md](cell-placement.md), [definition-config.md](../platform/definition-config.md), [depiction.md](depiction.md), [movement.md](../../../game/features/gameplay/movement.md).

## Requirements

- **`ActorDefinition`** (Types): `Id`, ordered accessory definitions, optional `DepictionConfig` / `IconConfig` / `DisplayName`, optional **`Tags`** / **`HasTag`**, optional **`Size`** (base projectile radius), optional starting **`Resources`** (`TagId` → amount) applied when the actor is constructed.
- **`Actor`** (Simulation): stable **`Id`** from a shared world allocator, accessories, flat `Effects` cache, resource bag, facing, **`FactionId`** (default **0**), optional `Cell`, **`Position`**, **`MoveIntent`**, **`AbilityLoadout`**, optional `DepictionOverride`, optional **`Projectile`** flight state (`IsProjectile`), definition ref, health helpers (`Health` / `MaxHealth` / `IsDestructible` / `IsAlive`), energy helpers. `AddAccessory` / `RemoveAccessory` sync effects, enablement, and loadout; run on-acquire effects.
- **`GameWorld`**: one **`Actors`** list for all live actors; **`CellActors`** occupancy map is a subset (every mapped actor is also in `Actors`). `TryPlaceActor` adds to both; free spawn (`AddActor`) adds to the list only; remove/death clears list and occupancy when `Cell` was set.
- **`IMoveEffect`** on accessories gates `ApplyMovement` (speed from the effect). Actors without a move effect ignore move intent.
- Passive ticks run over all actors (grow / spawn / world-passive / passive).
- JSON load: **`actors/`** after resources / domains / accessories (see [definition-config.md](../platform/definition-config.md)). Registry APIs: `AddActorDefinition` / `ActorDefinitions` / `TryGetActorDefinition`.
- **`GameContent.DefaultActor`** is the definition used for normal player spawns. Client syncs from `Actors` (cell vs cartesian presentation from `Cell` / `Position`); applies depiction override when set.

## Non-goals (for now)

- Separate character type or character definition catalog
