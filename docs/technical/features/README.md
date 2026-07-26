# Technical feature docs (read on demand)

**Source of truth** for architecture and simulation/client contracts. **Do not** open every file for general tasks.

Feature docs are grouped under `ui/`, `gameplay/`, `session/`, and `platform/` (further nesting may be added later).

1. Skim the **trigger** lines below.
2. If a trigger matches your current task, read **only** that markdown file (and linked paths as needed).

| File | Read when… |
|------|------------|
| [platform/error-handling.md](platform/error-handling.md) | Adding or changing **APIs**, loaders, boot paths, RPC, or any **multi-step** logic where failures must be chosen (throw vs explicit outcome vs abort). |
| [platform/testing.md](platform/testing.md) | Working on **automated tests**: unit vs functional layout, **xUnit**, protobuf gRPC Godot automation, `dotnet test`, **Godot-dependent** tests (`GODOT_BIN`), or **bug-driven regression** policy (failing test first / escalate brittle coverage). |
| [../technical-design.md](../technical-design.md) | **Architecture**, docs-as-SoT, simulation vs. client boundaries, **Godot directory layout**, TDD, or global-state rules. |
| [../../game/game-design.md](../../game/game-design.md) | Reading **gameplay vision**, genre pillars, co-op scope, or world/evolution feel. **Do not edit** unless the user explicitly instructed changes to that file. |
| [gameplay/controllers.md](gameplay/controllers.md) | Working on **IController**, possess/unpossess, PlayerController / AiController, **IMoveSteering**, or tick order. |
| [gameplay/navigation.md](gameplay/navigation.md) | Working on **navmesh**, Godot **NavigationAgent2D** / crowd avoidance, **Minimap.Simulation.Navigation**, or AI path steering. |
| [gameplay/actors.md](gameplay/actors.md) | Working on **Actor** / **ActorDefinition**, cell-anchored actors, or actor vs character hierarchy. |
| [gameplay/characters.md](gameplay/characters.md) | Working on **CharacterDefinition**, `GameContent.DefaultCharacter`, or character instantiation. |
| [gameplay/resources.md](gameplay/resources.md) | Working on **ResourceDefinition**, actor resource bags, limits, or effect use cost / acquire grant. |
| [gameplay/hunger.md](gameplay/hunger.md) | Working on **energy** / max_energy, `energy_upkeep`, Eat enable gate, drain/vitality passives, or instant-use Eat. |
| [gameplay/accessories.md](gameplay/accessories.md) | Working on **AccessoryDefinition**, effects, Gun, Farm, Geek, or actor effect cache add/remove. |
| [gameplay/active-abilities.md](gameplay/active-abilities.md) | Working on **AbilityLoadout**, dedicated/modal activation, or PlayerController ability intents. |
| [gameplay/interaction.md](gameplay/interaction.md) | Working on **IInteractionEffect**, interact target resolve, or Client highlight. |
| [gameplay/farming.md](gameplay/farming.md) | Working on Farm / grow / harvest effects, vegetable actors, or food yields. |
| [gameplay/cell-placement.md](gameplay/cell-placement.md) | Working on cell-anchored actors, occupancy, `ICellPlacementEffect`, or placement preview. |
| [gameplay/tags.md](gameplay/tags.md) | Working on **TagId**, `TagRegistry`, extension `RegisterTags`, definition tag strings, or resource type tags. |
| [gameplay/domains.md](gameplay/domains.md) | Working on **DomainDefinition**, domain JSON, domain color resolver, or domain-colored accessory icons. |
| [session/players.md](session/players.md) | Working on Simulation **`Player`**, accessory points, or controller↔player association. |
| [session/user-profiles.md](session/user-profiles.md) | Working on **player profiles** store, Profiles screen, lobby profile carousel, or death→profile counters. |
| [session/achievements.md](session/achievements.md) | Working on **achievement** catalog, unlock persistence, Survive 5 minutes tracking, or Achievements scene. |
| [ui/post-session.md](ui/post-session.md) | Working on **post-session** overlay, session achievement ledger UI, or all-ready → lobby. |
| [platform/definition-config.md](platform/definition-config.md) | Changing **accessory/character/actor/resource JSON** under an extension’s `config/` (e.g. CompuQuest), effect `type` map, or `DefinitionConfig` load APIs. |
| [gameplay/depiction.md](gameplay/depiction.md) | Working on **DepictionConfig**, SpriteFrames / texture depictions, or CompuQuest Kenney art under `assets/compuquest/`. |
| [ui/ui-icons.md](ui/ui-icons.md) | Working on **IconConfig**, definition `icon` JSON, or CompuQuest game-icons under `assets/compuquest/game-icons/`. |
| [gameplay/characters-and-factions.md](gameplay/characters-and-factions.md) | Changing the **Character** model, faction APIs, or spawn configuration parameters. |
| [ui/player-hud.md](ui/player-hud.md) | Working on **player HUD** panel, HUD models, resource rows, or App→Client HUD wiring. |
| [gameplay/missiles-and-damage.md](gameplay/missiles-and-damage.md) | Implementing **missile** / **SwingArc** lifecycle, hit tests, or damage application APIs. |
| [session/hex-grid-shape.md](session/hex-grid-shape.md) | Changing **HexGrid** rectangle inclusion, RadiusX/RadiusY, camera fit, or map geometry contracts. |
| [platform/core-settings.md](platform/core-settings.md) | Changing **core.json**, App settings load APIs, or the JSON vector-as-array convention. |
| [platform/extensions.md](platform/extensions.md) | Working on **extension loading**, `Minimap.Extensive`, `extensions.json`, integrators, or `CompuQuest.Minimap`. |
| [session/scenario-settings.md](session/scenario-settings.md) | Changing **scenario JSON**, CLI `--scenario`, `MINIMAP_SCENARIO`, or `ScenarioSettings` load APIs. |
| [platform/dotenv.md](platform/dotenv.md) | Adding or changing **`.env` / DotNetEnv** loading, `MINIMAP_DOTENV_DISABLED`, `START_SCREEN`, or manual-play env overrides. |
| [ui/lobby.md](ui/lobby.md) | Working on **lobby scene**, slot state machine, or lobby → world navigation. |
| [ui/main-menu.md](ui/main-menu.md) | Working on **main menu** screen/popup scenes, `START_SCREEN`, pause overlay, or exclusive menu control. |
| [session/local-input.md](session/local-input.md) | Working on **LocalInputAggregator**, joypad/keyboard routing, or reconnect overlay. |
| [session/local-play-context.md](session/local-play-context.md) | Working on **LocalPlayContext** autoload or cross-scene device roster. |
