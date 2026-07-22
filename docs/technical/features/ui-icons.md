# UI icons (technical)

Opaque UI icon references on content definitions. Implements [../../game/features/ui-icons.md](../../game/features/ui-icons.md). Related: [definition-config.md](definition-config.md), [depiction.md](depiction.md).

## Requirements

- **`IconConfig`** lives in **`Minimap.Simulation.Types`**: `ResourcePath` only. No Godot types. Simulation does not load textures.
- Optional on **`CharacterDefinition`** and **`AccessoryDefinition`** (`IconConfig?`).
- JSON field `"icon"`: `{ "path" }` parsed by **`DefinitionConfig`**. Omit or null → no icon.
- **Client** does not resolve or draw icons yet; upcoming UI sessions load `ResourcePath` as a Godot texture (SVG/PNG under `res://`).
- CompuQuest UI art under **`assets/compuquest/game-icons/`** (Godot `res://`; [game-icons.net](https://game-icons.net/) SVGs by author folder + `License.txt`, CC BY 3.0). Definition JSON stays under the extension `config/` tree. Paths look like `res://assets/compuquest/game-icons/{author}/{name}.svg`.
- Attribution: see pack `License.txt` (mention “Icons made by {author}”; site https://game-icons.net).

## Non-goals (for now)

- Client HUD / list UI that displays icons
- Validating `res://` existence inside App loaders
- Using game-icons for world depiction (Kenney / SpriteFrames remain depiction-only)
