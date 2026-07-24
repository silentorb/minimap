# Extensions

Minimap loads **extension** assemblies so game content can ship as libraries on top of the host. Contracts live in **`Minimap.Extensive`** (the extensibility capability assembly—not a plural bag of instances). Shipped lists of libraries live under plural paths (`config/extensions.json`, `extensions/`). Shared simulation **contracts** live in **`Minimap.Simulation.Types`** (not in Extensive; see that project’s `AGENTS.md`). Concrete accessory effects default to content extensions (CompuQuest).

## Roles

- **Extension** — a loadable C# library that implements `IExtension` and registers contributions via typed APIs on `IExtensionRegistry`.
- **Integrator** — the single authority for how registered contributions are turned into playthrough **`GameContent`**, plus content policy queries such as player-selectable accessories. **One active integrator per new game** (avoids conflicting multi-plugin integration).
- Typed registration only (e.g. `AddIntegrator`, `AddCharacterDefinition`, `RegisterTags`). There is **no** universal element type or normalized enumeration of “all registrable things.”
- **`GameContent`** (in Simulation.Types) is ordinary content for the rest of the app (`DefaultCharacter`, `WorldSpawnerPool`). Building it is an integration concern; consuming it is not.

## Requirements

- Extension contracts and the in-memory registry live in **`Minimap.Extensive`** (no Godot, no file I/O). Registry catalogs (registration order):
  - integrators
  - accessory effect factories (JSON `type` → `AccessoryEffect`)
  - resource definitions
  - accessory definitions
  - character definitions
  - placed-object definitions
  - tags (`TagRegistry` + `RegisterTags`)
- **`IIntegrator`**:
  - `CreateGameContent(IExtensionRegistry)` → `GameContent`
  - `GetPlayerSelectableAccessories(IExtensionRegistry)` → accessories players may choose
- **Minimap.App** loads settings and assemblies: `ExtensionsSettings`, `ExtensionLoader`. After load, App calls `CreateGameContent` and feeds **`GameContent`** (and selectable accessories) to Client via `WorldHostHooks`. Simulation has no I/O; Client does not load extension DLLs (hooks only).
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
  - `integrator` — id of the `IIntegrator` to use for the new game. Must be registered from a loaded extension or load fails.
- There is **no** built-in default integrator; CompuQuest registers id **`compuquest`**.
- `LobbyApp` (Client) loads extensions via `WorldHostHooks.RequireExtensions` on ready so a bad extension set fails before the player starts a game; also loads core accessory points for the selection budget.
- Sample content extension: **`CompuQuest.Minimap`** under `src/CompuQuest.Minimap`, built as a loadable DLL (**not** linked into the Godot host assembly). Depends on **Extensive** and **Simulation**. The host project (`minimap.csproj`) has a **build-only** `ProjectReference` (`ReferenceOutputAssembly=false`) so Godot Play / `dotnet build` builds it and copies output to repo-root `extensions/`. Registers integrator id **`compuquest`**, accessory effect factories (`shoot`, `place_random_actor`, `modify_resource`, `grow`, `harvest`), tag **`player_selectable`**, and ships accessory/character/actor/resource JSON under `src/CompuQuest.Minimap/config/` (copied to `extensions/CompuQuest.Minimap/` on build; see [definition-config.md](definition-config.md)); `ExtensionLoader` registers each extension’s content directory after that DLL’s `Register` and before `CreateGameContent`. CompuQuest presentation art lives under host **`assets/compuquest/`** (see [depiction.md](depiction.md), [ui-icons.md](ui-icons.md)).
- `CompuQuestIntegrator` sets `DefaultCharacter` to **`generic`**, builds a world spawner pool of **zombie spawners**, passes registered **resource** definitions into `GameContent` (including health / max_health tags), and filters selectable accessories by `player_selectable`.
- Lobby boot binds panels **before** extension load (`LobbySceneBoot`). A failed load **aborts** the lobby (no input / no further play) and quits; it must not leave a corrupted interactive scene.
- World boot (`WorldApp` / `WorldSceneBoot`) likewise fail-fast loads extensions (and core/scenario settings) via host hooks. On any exception during ready, abort (no tick / no reconnect), `GD.PushError`, and quit the process—same boundary pattern as lobby.

## Non-goals (for now)

- New-game UI to pick an integrator
- Hot-reload of extension assemblies
- Mixing multiple integrators in one run
- A universal “element” registration bag
