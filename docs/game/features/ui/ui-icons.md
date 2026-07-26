# UI icons

Icons for **UI display of data records** (definition lists, inventory panels, HUD resources, and similar). Technical: [ui-icons.md](../../../technical/features/ui/ui-icons.md). Distinct from [depiction](../gameplay/depiction.md) (world pawns / SpriteFrames). Related: [domains.md](../gameplay/domains.md).

## Requirements

- Character, accessory, and **resource** definitions may include an optional **icon** referencing a texture resource for UI.
- Short-term: CompuQuest ships icons from [game-icons.net](https://game-icons.net/) under `assets/compuquest/game-icons/` (CC BY 3.0; credit authors as required by that license).
- Icons are for data-record UI only—not for how characters or accessories look in the world (Kenney SpriteFrames remain the intended pawn path).
- **Prototype exception:** placed vegetables and animal companion characters may use game-icons SVGs as world **texture** depictions until dedicated SpriteFrames exist (see [cell-placement.md](../gameplay/cell-placement.md), [animal-companions.md](../gameplay/animal-companions.md)).
- The lobby **accessory selection panel** draws accessory icons from `IconConfig` paths. Domain-tagged accessories use a **domain color swatch** behind the white glyph (see [domains.md](../gameplay/domains.md)).
- The **player HUD** draws **visible** resource icons from resource-type `IconConfig` paths (see [player-hud.md](player-hud.md), [resources.md](../gameplay/resources.md)), and the selected ability icon with the same domain swatch rules when applicable.

## Non-goals (for now)

- Permanent use of game-icons as character pawn sprites (prototype placed-object textures only)
