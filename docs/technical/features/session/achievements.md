# Achievements (technical)

Implements [achievements.md](../../../game/features/session/achievements.md). Related: [user-profiles.md](user-profiles.md), [../ui/post-session.md](../ui/post-session.md), [local-play-context.md](local-play-context.md).

## Requirements

- **Catalog**: static `AchievementCatalog` / ids in Client (e.g. `survive_5_minutes` → title **Survive 5 minutes**). Definitions are code-owned for the first cut (not JSON).
- **Persistence**: extend `user://player_profiles.json` profile entries with `unlockedAchievements` (string id array). Missing field → none unlocked. Unknown ids on load are kept (forward-compatible) but not shown unless present in the catalog. Mutations save immediately via existing profile store hooks.
- **Survive 5 minutes**: `GameSession` accumulates per-human consecutive alive sim seconds (`dt` while pawn alive and not game-over). Death or removal resets that human’s consecutive timer. When a human crosses **300** seconds, `GameSession` exposes a this-tick unlock signal (player index). Client maps to roster `ProfileId`, records a **session earn** (with first-time = not previously on profile), and if first-time calls catalog unlock + save.
- **Achievements scene**: `res://scenes/achievements.tscn` — root under `Minimap.Client.Profiles`. Profile id passed via `LocalPlayContext` focus field set by Profiles before `ChangeSceneToFile`. Back → `profiles.tscn`.
- **Session ledger**: `WorldApp` keeps per-player session earns for the [post-session](../ui/post-session.md) screen (includes re-earns with `FirstTime == false`).

## Non-goals (for now)

- Achievement definition JSON / extension registration
- Simulation referencing profile store (unlock persistence stays Client/App)
