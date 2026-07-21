# Agent notes — Minimap.App

## Purpose

Thin composition between Simulation, Simulation.Navigation, and Client: session creation (`GameSession`, `GameApp`), attach player controllers, wire Godot navigation steering for AI, feed HUD models. Owns loading settings and extension assemblies from config. Sources are compiled into the Godot host assembly.

## What may live here

- Boot / session wiring (`GameApp`, `GameSession`, `WorldSceneBoot`, …), including parenting/rebuilding **Minimap.Simulation.Navigation** and upgrading AI `IMoveSteering`
- Settings load APIs (`CoreSettings`, `ScenarioSettings`, `DefinitionSettings`, `ExtensionsSettings`)
- Extension loading (`ExtensionLoader`) and registering Client hooks (`AppHostRegistration` → `ExtensionPreflight`)
- World fail-fast abort boundaries (`GameApp` / `WorldSceneBoot`)

## What must not live here

- Deep gameplay rules (belong in Simulation)
- `Control` or similar Godot UI classes / lobby scene roots / HUD widgets (belong in Client)
- Concrete extension content / sealed accessory effects (belong in content extensions)

Simulation and Client do not load extension DLLs or settings files—**App owns that I/O**. See [extensions.md](../../docs/technical/features/extensions.md), [core-settings.md](../../docs/technical/features/core-settings.md).
