# UI icons (technical)

Opaque UI icon references on content definitions. Implements [../../game/features/ui-icons.md](../../game/features/ui-icons.md). Related: [definition-config.md](definition-config.md), [depiction.md](depiction.md), [resources.md](resources.md), [player-hud.md](player-hud.md).

## Requirements

- **`IconConfig`** lives in **`Minimap.Simulation.Types`**: `ResourcePath` only. No Godot types. Simulation does not load textures.
- Optional on **`CharacterDefinition`**, **`AccessoryDefinition`**, and **`ResourceDefinition`** (`IconConfig?`).
- JSON field `"icon"`: `{ "path" }` parsed by **`DefinitionConfig`**. Omit or null → no icon.
- **Client** lobby accessory selection panel and **player HUD** resource rows load `ResourcePath` as a Godot texture for icons.
- CompuQuest UI art under **`assets/compuquest/game-icons/`** (Godot `res://`; [game-icons.net](https://game-icons.net/) SVGs by author folder + `License.txt`, CC BY 3.0). Definition JSON stays under the extension `config/` tree. Paths look like `res://assets/compuquest/game-icons/{author}/{name}.svg`.
- Attribution: see pack `License.txt` (mention “Icons made by {author}”; site https://game-icons.net).

## Non-goals (for now)

- Validating `res://` existence inside App loaders
- Using game-icons for world depiction (Kenney / SpriteFrames remain depiction-only; prototype placed-object textures excepted per game docs)
