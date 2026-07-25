# Actors

General runtime entities in the world. Related: [characters.md](characters.md), [accessories.md](accessories.md), [health.md](health.md), [farming.md](farming.md), [interaction.md](interaction.md), [cell-placement.md](cell-placement.md), [depiction.md](depiction.md). Technical: [actors.md](../../../technical/features/gameplay/actors.md).

## Requirements

- An **actor** is a runtime entity with identity, accessories/effects, resources, facing, and depiction (including an optional runtime depiction override).
- An **actor definition** describes what to spawn: id, optional display name / depiction / icon, ordered accessory definitions applied at instantiation, and optional **starting resources** (e.g. health / max health for placeables).
- **Characters** are actors specialized for possessable mobile pawns (faction, movement, ability loadout). Health helpers live on the actor base; characters always initialize destructible health. See [characters.md](characters.md) and [health.md](health.md).
- **Cell-anchored actors** occupy a single map cell (former “placed objects”). Occupancy tracks these actors only (not mobile characters). Placement snaps to cell centers. Destructible placeables die and are removed when health reaches 0 (same quiet removal as characters).
- Actor definitions for placeable content ship as JSON under the content extension (`src/CompuQuest.Minimap/config/actors/`).
- Behavior on actors comes from **effects** on their accessories, same as characters.

## Non-goals (for now)

- Occupancy blocking character movement
- Actors that are both freely mobile and cell-anchored at once
