# UI icons

Icons for **UI display of data records** (definition lists, inventory panels, and similar). Technical: [../../technical/features/ui-icons.md](../../technical/features/ui-icons.md). Distinct from [depiction](depiction.md) (world pawns / SpriteFrames).

## Requirements

- Character and accessory definitions may include an optional **icon** referencing a texture resource for UI.
- Short-term: CompuQuest ships icons from [game-icons.net](https://game-icons.net/) under `assets/compuquest/game-icons/` (CC BY 3.0; credit authors as required by that license).
- Icons are for data-record UI only—not for how characters or accessories look in the world.
- The lobby **accessory selection panel** draws accessory icons from `IconConfig` paths (TextureRect / Button icons).

## Non-goals (for now)

- Drawing icons in the player HUD (definitions still carry the path)
- Using game-icons art as world depiction / pawn sprites
