# Technical feature docs (read on demand)

**Source of truth** for architecture and simulation/client contracts. Do **not** open every file for general tasks.

1. Skim the **trigger** lines below.
2. If a trigger matches your current task, read **only** that markdown file (and linked paths as needed).

| File | Read when… |
|------|------------|
| [error-handling.md](error-handling.md) | Adding or changing **APIs**, loaders, boot paths, RPC, or any **multi-step** logic where failures must be chosen (throw vs explicit outcome vs abort). |
| [testing.md](testing.md) | Working on **automated tests**: unit vs functional layout, **xUnit**, protobuf gRPC Godot automation, `dotnet test`, **Godot-dependent** tests (`GODOT_BIN`), or **bug-driven regression** policy (failing test first / escalate brittle coverage). |
| [../technical-design.md](../technical-design.md) | **Architecture**, docs-as-SoT, simulation vs. client boundaries, **Godot directory layout**, TDD, or global-state rules. |
| [../../game/game-design.md](../../game/game-design.md) | Changing **gameplay vision**, genre pillars, co-op scope, or world/evolution feel. |
| [controllers.md](controllers.md) | Working on **IController**, possess/unpossess, PlayerController / AiController, or tick order. |
| [characters.md](characters.md) | Working on **CharacterDefinition**, `GameContent.DefaultCharacter`, or character instantiation. |
| [accessories.md](accessories.md) | Working on **AccessoryDefinition**, effects, Gun, or `Character` effect cache add/remove. |
| [characters-and-factions.md](characters-and-factions.md) | Changing the **Character** model, faction APIs, or spawn configuration parameters. |
| [player-hud.md](player-hud.md) | Working on **player HUD** panel, HUD models, or App→Client HUD wiring. |
| [missiles-and-damage.md](missiles-and-damage.md) | Implementing **missile** lifecycle, hit tests, or damage application APIs. |
| [hex-grid-shape.md](hex-grid-shape.md) | Changing **HexGrid** rectangle inclusion, RadiusX/RadiusY, camera fit, or map geometry contracts. |
| [core-settings.md](core-settings.md) | Changing **core.json**, App settings load APIs, or the JSON vector-as-array convention. |
| [extensions.md](extensions.md) | Working on **extension loading**, `Minimap.Extensive`, `extensions.json`, integrators, or `CompuQuest.Minimap`. |
| [scenario-settings.md](scenario-settings.md) | Changing **scenario JSON**, CLI `--scenario`, or `ScenarioSettings` load APIs. |
| [lobby.md](lobby.md) | Working on **lobby scene**, slot state machine, or lobby → world navigation. |
| [local-input.md](local-input.md) | Working on **LocalInputAggregator**, joypad/keyboard routing, or reconnect overlay. |
| [local-play-context.md](local-play-context.md) | Working on **LocalPlayContext** autoload or cross-scene device roster. |
