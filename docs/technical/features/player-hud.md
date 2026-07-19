# Player HUD (technical)

Client UI and App wiring for local player status. Implements [../../game/features/player-hud.md](../../game/features/player-hud.md). Related: [controllers.md](controllers.md), [characters-and-factions.md](characters-and-factions.md).

## Requirements

- **All HUD UI** lives in **Minimap.Client** (`PlayerHud`, `PlayerHudPanel`, `PlayerHudModel`, scenes under `ui/`).
- HUD models contain **no Simulation types** — only display name, health, and max health.
- **Minimap.App** (`GameSession.BuildHudModels`) maps Client `PlayerController` pawns → `PlayerHudModel` and pushes snapshots into `PlayerHudPanel`.
- Slot count matches **local human player count** (1–4), configured at the App surface (`GameApp.LocalPlayerCount` / `SpawnConfig.HumanPlayerCount`).
- Panel docks at the **bottom** of the viewport (`CanvasLayer` + bottom-anchored container).

## Non-goals (for now)

- Reading `Character` directly from HUD controls
- Networked remote player HUD slots
