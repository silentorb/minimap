# Cell placement

Placing cell-anchored **actors** on map **cells** (spatial partitions). Related: [actors.md](actors.md), [active-abilities.md](active-abilities.md), [accessories.md](accessories.md), [farming.md](farming.md), [map-layout.md](../session/map-layout.md), [depiction.md](depiction.md). Technical: [cell-placement.md](../../../technical/features/gameplay/cell-placement.md).

## Terminology

- **Cell** — a hex spatial partition (`HexAxial`) with a terrain type.
- **Tile** — depiction / tilesheet art, not the grid partition.

## Requirements

- Characters may place **cell-anchored actors** onto individual cells. Placement snaps to cell centers so actors cannot be crammed together.
- Occupancy tracks **cell-anchored actors only** (not mobile characters).
- General placement is engine-owned; content ability effects supply validation and what gets placed (weighted pool of actor definitions).
- **Farm** (CompuQuest): modal ability; plant effect costs **seeds**; places a random vegetable actor on an **unoccupied Grass** cell in front of the player. See [farming.md](farming.md).
- **Geek** (CompuQuest): modal ability; place effect costs **electronics**; places a **computer** actor on an **unoccupied Grass** cell in front of the player (inherits the placer’s faction). Computers are immobile **turrets**: intrinsic gun accessory, **10** starting ammo, auto-aim at nearest hostile and fire like AI gunners. With Geek selected, environment interact targets a computer in the front cell (costs **1** energy).
- Placement UX (players): first modal activate enters **preview** (highlight front cell — greenish if valid, redder if invalid); second activate places if valid; **Back** cancels preview. (General two-step activate flow; see [active-abilities.md](active-abilities.md).)
- Facing: last non-zero move direction; a short aim line shows facing; the front cell is the neighbor in that facing.

## Terrain

- Playable walkable terrain is **Grass** (formerly Floor). **Hazard** cells are removed.
- Walls remain impassable / non-placeable for Farm / Geek placement.

## Non-goals (for now)

- Changing cell terrain type via abilities (later)
- Occupancy blocking character movement
