# Cell placement

Placing static objects on map **cells** (spatial partitions). Related: [active-abilities.md](active-abilities.md), [accessories.md](accessories.md), [map-layout.md](map-layout.md), [depiction.md](depiction.md). Technical: [../../technical/features/cell-placement.md](../../technical/features/cell-placement.md).

## Terminology

- **Cell** — a hex spatial partition (`HexAxial`) with a terrain type.
- **Tile** — depiction / tilesheet art, not the grid partition.

## Requirements

- Characters may place **static objects** onto individual cells. Placement snaps to cell centers so objects cannot be crammed together.
- Occupancy tracks **placed objects only** (not mobile characters).
- General placement is engine-owned; content accessories supply validation and what gets placed.
- **Plant Vegetable** (CompuQuest): modal ability; consumes **seeds**; places a random vegetable from a pool (carrot, corn, melon) on an **unoccupied Grass** cell in front of the player.
- **Geek** (CompuQuest): modal ability; consumes **computers**; places a **computer** on an **unoccupied Grass** cell in front of the player (no further computer behavior yet).
- Placement UX (players): first activate enters **preview** (highlight front cell — greenish if valid, redder if invalid); second activate places if valid; **Back** cancels preview.
- Facing: last non-zero move direction; a short aim line shows facing; the front cell is the neighbor in that facing.

## Terrain

- Playable walkable terrain is **Grass** (formerly Floor). **Hazard** cells are removed.
- Walls remain impassable / non-placeable for Plant Vegetable.

## Non-goals (for now)

- Harvesting crops or growth stages / sprouts
- Changing cell terrain type via abilities (later)
- Occupancy blocking character movement
- Object–object interaction (outside placement)
