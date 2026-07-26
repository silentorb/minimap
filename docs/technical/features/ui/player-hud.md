# Player HUD (technical)

Client UI and local session wiring for player status. Implements [player-hud.md](../../../game/features/ui/player-hud.md). Related: [controllers.md](../gameplay/controllers.md), [characters-and-factions.md](../gameplay/characters-and-factions.md), [resources.md](../gameplay/resources.md), [ui-icons.md](ui-icons.md), [active-abilities.md](../gameplay/active-abilities.md), [hunger.md](../gameplay/hunger.md).

## Requirements

- **All HUD UI** lives in **Minimap.Client** (`PlayerHud`, `PlayerHudPanel`, `PlayerHudModel`, scenes under `ui/`).
- HUD models contain **no Simulation types** — display name, optional absolute avatar path, a list of resource rows (`DisplayName`, `IconPath`, `Amount`, optional `MaxAmount`), and optional selected modal ability (`DisplayName`, `IconPath`; null/absent when none).
- **Minimap.Client** (`ClientSession.BuildHudModels`) maps `PlayerController` pawns + `GameContent.Resources` → `PlayerHudModel` (including selected modal definition icon/name from the filtered loadout) and `WorldApp` pushes snapshots into `PlayerHudPanel`. Display names and optional avatar paths come from the lobby roster [user profile](../session/user-profiles.md) when present; otherwise `"Player N"` with no avatar.
- Name row: ≈24–32px avatar `TextureRect` + name label; hide the avatar control when the path is missing or the image fails to load.
- Only **`Visible`** resource types appear; sort by **`UiPriority` descending**; limited resources include `MaxAmount` from the limit tag amount.
- Slot count matches **local human player count** (1–4), from lobby roster or `WorldApp.LocalPlayerCount` when world is loaded directly.
- Panel docks at the **bottom** of the viewport (`CanvasLayer` + bottom-anchored container). Truncate to **4** resource rows + `+N` overflow. Equipped-ability row shows **32×32** icon + label; hide when no selected modal.

## Non-goals (for now)

- Reading `Character` directly from HUD controls
- Networked remote player HUD slots
