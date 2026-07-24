# Cell placement (technical)

Occupancy and placement APIs. Implements [../../game/features/cell-placement.md](../../game/features/cell-placement.md). Related: [active-abilities.md](active-abilities.md), [accessories.md](accessories.md), [hex-grid-shape.md](hex-grid-shape.md), [depiction.md](depiction.md).

## Requirements

- **`PlacedObjectDefinition`** (Types) + **`PlacedObject`** (Simulation): static cell occupants with depiction.
- **`GameWorld`**: `PlacedObjects` occupancy map; `TryPlaceObject` / `TryRemoveObjectAt` / `IsCellOccupied` return false for expected rejection (do not throw).
- **`ICellPlacementEffect`** (Simulation): `CanPlace` / `TryPlace` for accessory effects. CompuQuest **`PlaceRandomObjectEffect`** (`place_random_object`) picks from a pool of placed-object ids registered on the world. Placement also gates on the accessory’s consumed resource (see [resources.md](resources.md)).
- **`CellFacing`**: facing vector → hex neighbor offset → cell in front of a character.
- **`Character.Facing`**: updated from non-zero move intent during movement apply.
- Client: placement preview tint on hex polygons; texture depiction kind for prototype vegetable art; placed-object layer sync.

## Non-goals (for now)

- Shared `Result<T>` library for placement outcomes
- Navmesh / collision contribution from placed objects
