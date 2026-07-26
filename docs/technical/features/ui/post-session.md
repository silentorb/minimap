# Post-session summary (technical)

Implements [post-session.md](../../../game/features/ui/post-session.md). Related: [../session/achievements.md](../session/achievements.md), [lobby.md](lobby.md), [main-menu.md](main-menu.md), [../session/local-play-context.md](../session/local-play-context.md).

## Requirements

- **UI**: replace `GameOverOverlay` with `PostSessionOverlay` (`res://ui/post_session_overlay.tscn`, Client). Full-screen dimmed layer; per-player columns/panels with name, session achievement list (first-time badge), and Ready affordance. `process_mode = Always`.
- **Show**: `WorldApp` on `GameSession.IsGameOver` or pause-menu **End game** — set gameplay paused, hide main-menu popup, show overlay with session ledger snapshot. Do not change scene until all ready.
- **Ready gate**: pure model (unit-tested) tracks per-player ready; input from each roster device toggles/sets ready (mirror lobby Ready activate). When all ready → `ChangeSceneToFile("res://scenes/lobby.tscn")` **without** clearing the roster first so lobby can hydrate.
- **LocalPlayContext**: set a **returning-from-session** flag (or equivalent) before leaving world so lobby skips fresh `Clear()` and calls restore. Main menu **New** still clears for a fresh lobby.
- **Automation**: replace/adapt `ForceGameOverForTests` / game-over playbooks to assert post-session → all ready → lobby (not main menu from this screen).

## Non-goals (for now)

- Separate scene load for summary (overlay on world is enough)
- Direct world → `main_menu.tscn` from this overlay
