# Extensions

Minimap loads **extension** assemblies so game content can ship as libraries on top of the host. Contracts live in **`Minimap.Extensive`** (the extensibility capability assembly—not a plural bag of instances). Shipped lists of libraries live under plural paths (`config/extensions.json`, `extensions/`).

## Roles

- **Extension** — a loadable C# library that implements `IExtension` and registers contributions via typed APIs on `IExtensionRegistry`.
- **Integrator** — the single authority for how registered contributions are used in a playthrough. **One active integrator per new game** (avoids conflicting multi-plugin integration).
- Typed registration only (e.g. `AddIntegrator`). There is **no** universal element type or normalized enumeration of “all registrable things.”

## Requirements

- Extension contracts and the in-memory registry live in **`Minimap.Extensive`** (no Godot, no file I/O).
- **Minimap.App** loads settings and assemblies: `ExtensionsSettings`, `ExtensionLoader`. Simulation and Client do not load extension DLLs.
- Shipped config: **`config/extensions.json`** (Godot path `res://config/extensions.json`).
- Schema:

```json
{
  "searchPaths": ["extensions"],
  "extensions": ["CompuQuest.Minimap.dll"],
  "integrator": "compuquest"
}
```

  - `searchPaths` — directories used to resolve non-absolute extension entries. Relative paths are tried against the config file’s directory, that directory’s parent (project root when config is under `config/`), and the process current directory; the first existing file wins.
  - `extensions` — each entry is an absolute path or a filename/relative name resolved via `searchPaths`. Missing libraries fail fast.
  - `integrator` — id of the `IIntegrator` to use for the new game. Must be registered (built-in or from a loaded extension) or load fails.
- Built-in **`DefaultIntegrator`** with id **`default`** is always registered before extension DLLs load.
- `IIntegrator` currently exposes only **`Id`**. Further integrator APIs (e.g. listing components by type) come later.
- `GameApp` loads extensions on ready (exportable path, default `res://config/extensions.json`) and passes the selected integrator into `GameSession` (stored for future use; no gameplay behavior change from the integrator yet).
- `LobbyApp` also loads the same config on ready so a bad extension set fails before the player starts a game.
- Sample content extension: **`CompuQuest.Minimap`** under `src/CompuQuest.Minimap`, built as a loadable DLL (not referenced by the Godot host). Its `CompuQuestExtension` registers integrator id **`compuquest`**. Build output is copied to repo-root `extensions/`.

## Non-goals (for now)

- Component type catalogs (characters, abilities, tiles, scenarios) and matching integrator getters
- New-game UI to pick an integrator
- Hot-reload of extension assemblies
- Mixing multiple integrators in one run
- A universal “element” registration bag
