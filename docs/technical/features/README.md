# Technical feature docs (read on demand)

**Source of truth** for architecture and simulation/client contracts. Do **not** open every file for general tasks.

1. Skim the **trigger** lines below.
2. If a trigger matches your current task, read **only** that markdown file (and linked paths as needed).

| File | Read when… |
|------|------------|
| [testing.md](testing.md) | Working on **automated tests**: unit vs functional layout, **xUnit**, protobuf gRPC Godot automation, `dotnet test`, or running **Godot-dependent** tests from the CI / **dev container** (`GODOT_BIN`). |
| [../technical-design.md](../technical-design.md) | **Architecture**, docs-as-SoT, simulation vs. client boundaries, **Godot directory layout**, TDD, or global-state rules. |
| [../../game/game-design.md](../../game/game-design.md) | Changing **gameplay vision**, genre pillars, co-op scope, or world/evolution feel. |
| [controllers.md](controllers.md) | Working on **IController**, possess/unpossess, PlayerController / AiController, or tick order. |
| [characters-and-factions.md](characters-and-factions.md) | Changing the **Character** model, faction APIs, or spawn configuration parameters. |
| [player-hud.md](player-hud.md) | Working on **player HUD** panel, HUD models, or App→Client HUD wiring. |
| [missiles-and-damage.md](missiles-and-damage.md) | Implementing **missile** lifecycle, hit tests, or damage application APIs. |
| [hex-grid-shape.md](hex-grid-shape.md) | Changing **HexGrid** ellipse inclusion, RadiusX/RadiusY, or map geometry contracts. |
| [core-settings.md](core-settings.md) | Changing **core.json**, App settings load APIs, or the JSON vector-as-array convention. |
