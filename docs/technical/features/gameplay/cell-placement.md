# Cell placement (technical)

Occupancy and placement APIs. Implements [cell-placement.md](../../../game/features/gameplay/cell-placement.md). Related: [actors.md](actors.md), [active-abilities.md](active-abilities.md), [accessories.md](accessories.md), [farming.md](farming.md), [hex-grid-shape.md](../session/hex-grid-shape.md), [depiction.md](depiction.md).

## Requirements

- Cell-anchored **`Actor`** + **`ActorDefinition`** (see [actors.md](actors.md)): occupancy map on **`GameWorld`**; `TryPlaceActor` / `TryRemoveActorAt` / `IsCellOccupied` return false for expected rejection (do not throw).
- **`ICellPlacementEffect`** (Simulation): `CanPlace` / `TryPlace` for accessory effects. CompuQuest **`PlaceRandomActorEffect`** (`place_random_actor`) picks from a **weighted pool** of actor definition ids registered on the world. Placement gates/consumes via the effect’s **`IEffectUseCost`** (see [resources.md](resources.md)).
- **`CellFacing`**: facing vector → hex neighbor offset → cell in front of a character.
- **`Actor.Facing`**: controllers update it (players from non-zero aim; AI from aim else move intent). Movement apply does not change facing.
- Client: placement preview tint on hex polygons; texture depiction for prototype vegetable art; cell-actor layer sync with depiction override support.

## Non-goals (for now)

- Shared `Result<T>` library for placement outcomes
- Navmesh / collision contribution from cell actors
