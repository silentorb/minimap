# Extensions

Minimap loads **extension** assemblies so game content can ship as libraries on top of the host. Contracts live in **`Minimap.Extensive`** (the extensibility capability assembly—not a plural bag of instances). Shipped lists of libraries live under plural paths (`config/extensions.json`, `extensions/`). Shared simulation **contracts** live in **`Minimap.Simulation.Types`** (not in Extensive; see that project’s `AGENTS.md`). Concrete accessory effects default to content extensions (CompuQuest).

## Roles

- **Extension** — a loadable C# library that implements `IExtension` and registers contributions via typed APIs on `IExtensionRegistry`.
- **Integrator** — the single authority for how registered contributions are turned into playthrough **`GameContent`**. **One active integrator per new game** (avoids conflicting multi-plugin integration).
- Typed registration only (e.g. `AddIntegrator`, `AddCharacterDefinition`). There is **no** universal element type or normalized enumeration of “all registrable things.”
- **`GameContent`** (in Simulation.Types) is ordinary content for the rest of the app (e.g. `DefaultCharacter`). Building it is an integration concern; consuming it is not.

## Requirements

- Extension contracts and the in-memory registry live in **`Minimap.Extensive`** (no Godot, no file I/O). Registry catalogs (registration order):
  - integrators
  - accessory effect factories (JSON `type` → `AccessoryEffect`)
  - accessory definitions
  - character definitions
- **`IIntegrator.CreateGameContent(IExtensionRegistry)`** returns `GameContent`. **`DefaultIntegrator`** sets `DefaultCharacter` to the **first registered** character definition (fails if none).
- **Minimap.App** loads settings and assemblies: `ExtensionsSettings`, `ExtensionLoader`. After load, App calls `CreateGameContent` and feeds **`GameContent`** to Client via `WorldHostHooks` for Simulation `GameSession` create. Simulation has no I/O; Client does not load extension DLLs (hooks only).
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
- `LobbyApp` (Client) also preflight-loads the same config on ready via `ExtensionPreflight` (App registers `ExtensionLoader`) so a bad extension set fails before the player starts a game.
- Sample content extension: **`CompuQuest.Minimap`** under `src/CompuQuest.Minimap`, built as a loadable DLL (**not** linked into the Godot host assembly). Depends on **Extensive** and **Simulation**. The host project (`minimap.csproj`) has a **build-only** `ProjectReference` (`ReferenceOutputAssembly=false`) so Godot Play / `dotnet build` builds it and copies output to repo-root `extensions/`. Registers integrator id **`compuquest`**, the **`shoot`** accessory effect factory, and ships accessory/character JSON under `src/CompuQuest.Minimap/config/` (copied to `extensions/CompuQuest.Minimap/` on build; see [definition-config.md](definition-config.md)); `ExtensionLoader` registers each extension’s content directory after that DLL’s `Register` and before `CreateGameContent`. CompuQuest presentation art lives under host **`assets/compuquest/`** (see [depiction.md](depiction.md), [ui-icons.md](ui-icons.md)).
- Lobby boot binds panels **before** extension preflight (`LobbySceneBoot`). A failed load **aborts** the lobby (no input / no further play) and quits; it must not leave a corrupted interactive scene.
- World boot (`WorldApp` / `WorldSceneBoot`) likewise fail-fast loads extensions (and core/scenario settings) via host hooks. On any exception during ready, abort (no tick / no reconnect), `GD.PushError`, and quit the process—same boundary pattern as lobby.

## Non-goals (for now)

- New-game UI to pick an integrator
- Hot-reload of extension assemblies
- Mixing multiple integrators in one run
- A universal “element” registration bag
- Per-accessory query APIs on the integrator (use registry catalogs / character definitions)
