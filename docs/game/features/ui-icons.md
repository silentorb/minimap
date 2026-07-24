# UI icons

Icons for **UI display of data records** (definition lists, inventory panels, HUD resources, and similar). Technical: [../../technical/features/ui-icons.md](../../technical/features/ui-icons.md). Distinct from [depiction](depiction.md) (world pawns / SpriteFrames).

## Requirements

- Character, accessory, and **resource** definitions may include an optional **icon** referencing a texture resource for UI.
- Short-term: CompuQuest ships icons from [game-icons.net](https://game-icons.net/) under `assets/compuquest/game-icons/` (CC BY 3.0; credit authors as required by that license).
- Icons are for data-record UI only—not for how characters or accessories look in the world (Kenney SpriteFrames remain the intended pawn path).
- **Prototype exception:** placed vegetables may use game-icons SVGs as world **texture** depictions until dedicated crop SpriteFrames exist (see [cell-placement.md](cell-placement.md)).
- The lobby **accessory selection panel** draws accessory icons from `IconConfig` paths (TextureRect / Button icons).
- The **player HUD** draws **visible** resource icons from resource-type `IconConfig` paths (see [player-hud.md](player-hud.md), [resources.md](resources.md)).

## Non-goals (for now)

- Permanent use of game-icons as character pawn sprites (prototype placed-object textures only)
