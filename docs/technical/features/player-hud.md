# Player HUD (technical)

Client UI and local session wiring for player status. Implements [../../game/features/player-hud.md](../../game/features/player-hud.md). Related: [controllers.md](controllers.md), [characters-and-factions.md](characters-and-factions.md).

## Requirements

- **All HUD UI** lives in **Minimap.Client** (`PlayerHud`, `PlayerHudPanel`, `PlayerHudModel`, scenes under `ui/`).
- HUD models contain **no Simulation types** — only display name, health, and max health.
- **Minimap.Client** (`ClientSession.BuildHudModels`) maps `PlayerController` pawns → `PlayerHudModel` and `WorldApp` pushes snapshots into `PlayerHudPanel`.
- Slot count matches **local human player count** (1–4), from lobby roster or `WorldApp.LocalPlayerCount` when world is loaded directly.
- Panel docks at the **bottom** of the viewport (`CanvasLayer` + bottom-anchored container).

## Non-goals (for now)

- Reading `Character` directly from HUD controls
- Networked remote player HUD slots
