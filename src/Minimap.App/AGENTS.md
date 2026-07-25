# Agent notes — Minimap.App

## Purpose

Host-side settings and extension **file I/O**, plus ModuleInitializer hooks so Client scene roots can load config without referencing App types. Sources are compiled into the Godot host assembly.

## What may live here

- Settings/config load APIs (`CoreSettings`, `ScenarioSettings`, `DefinitionConfig`, `ExtensionsSettings`)
- Extension loading (`ExtensionLoader`, `ExtensionPathResolver`) and registering Client hooks (`AppHostRegistration` → `ExtensionPreflight`, `WorldHostHooks`)
- Extension content deploy helper (`ExtensionContentMirror` — wipe+copy JSON trees for gitignored `extensions/*/`)
- CLI / env bootstrap parsing (`CliArgs`) and optional dotenv (`DotEnvBootstrap` — manual play only; never in tests)

## What must not live here

- Godot scene roots / `Node` scripts (belong in Client: `LobbyApp`, `WorldApp`)
- Authoritative playthrough session (`GameSession` belongs in Simulation)
- Client controllers, HUD, input, reconnect UI
- Deep gameplay rules (belong in Simulation)
- Concrete extension content / sealed accessory effects (belong in content extensions)

Simulation has **no I/O**. Client may do light device/engine I/O but must not load shipped settings/extension files—**App owns that file I/O**. See [extensions.md](../../docs/technical/features/extensions.md), [core-settings.md](../../docs/technical/features/core-settings.md).
