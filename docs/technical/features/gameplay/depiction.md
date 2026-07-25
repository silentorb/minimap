# Depiction (technical)

Opaque visual presentation references on content definitions. Implements [depiction.md](../../../game/features/gameplay/depiction.md). Related: [characters.md](characters.md), [accessories.md](accessories.md), [definition-config.md](../platform/definition-config.md).

## Requirements

- **`DepictionConfig`** lives in **`Minimap.Simulation.Types`**: `Kind`, `ResourcePath`, optional `DefaultAnimation`. No Godot types.
- **`DepictionKinds.SpriteFrames`** (`"sprite_frames"`) means `ResourcePath` is a Godot `SpriteFrames` `.tres`.
- **`DepictionKinds.Texture`** (`"texture"`) means `ResourcePath` is a static texture (including imported SVG); used for prototype placed-object art.
- Optional on **`CharacterDefinition`** and **`AccessoryDefinition`** (`DepictionConfig?`).
- JSON field `"depiction"`: `{ "kind", "path", "animation"? }` parsed by **`DefinitionConfig`**. Omit or null → no depiction.
- **Client** (`WorldView`): for characters with `kind == sprite_frames`, load `SpriteFrames`, drive `AnimatedSprite2D` on `player_visual.tscn`, hide the ColorRect placeholder; on missing/unknown kind, keep ColorRect + faction tint. Scale 16×16 tiles by **1.25** so pawns stay near the former ~20px footprint.
- CompuQuest art under **`assets/compuquest/kenney-1bit/`** (Godot `res://`; Kenney 1-Bit Pack CC0 tilesheet + `depict/*.tres`). Definition JSON stays under the extension `config/` tree.

## Non-goals (for now)

- Drawing accessory depictions as equip overlay on the pawn (data-record / HUD icons: [ui-icons.md](../ui/ui-icons.md))
- Death VFX / additional depiction kinds beyond the discriminator
- Validating `res://` existence inside App loaders
