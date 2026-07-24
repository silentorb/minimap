# Actors

General runtime entities in the world. Related: [characters.md](characters.md), [accessories.md](accessories.md), [farming.md](farming.md), [interaction.md](interaction.md), [cell-placement.md](cell-placement.md), [depiction.md](depiction.md). Technical: [../../technical/features/actors.md](../../technical/features/actors.md).

## Requirements

- An **actor** is a runtime entity with identity, accessories/effects, resources, facing, and depiction (including an optional runtime depiction override).
- An **actor definition** describes what to spawn: id, optional display name / depiction / icon, and ordered accessory definitions applied at instantiation.
- **Characters** are actors specialized for possessable mobile pawns (faction, movement, ability loadout, health helpers). See [characters.md](characters.md).
- **Cell-anchored actors** occupy a single map cell (former “placed objects”). Occupancy tracks these actors only (not mobile characters). Placement snaps to cell centers.
- Actor definitions for placeable content ship as JSON under the content extension (`src/CompuQuest.Minimap/config/actors/`).
- Behavior on actors comes from **effects** on their accessories, same as characters.

## Non-goals (for now)

- Occupancy blocking character movement
- Actors that are both freely mobile and cell-anchored at once
