# Game over

All human players dead. Related: [../gameplay/health.md](../gameplay/health.md), [../session/scenarios.md](../session/scenarios.md), [lobby.md](lobby.md), [main-menu.md](main-menu.md).

## Requirements

- When **every local human player** has died (pawn removed or health ≤ 0), the game enters **game over**.
- Gameplay pauses and a **Game Over** overlay appears with:
  - **New game** — return to the lobby (`lobby.tscn`) to start a fresh local session.
  - **Main menu** — return to the main menu screen (`main_menu.tscn`).
- Level-end heal/resurrect (between waves levels) is separate from game over; see [scenarios.md](../session/scenarios.md).

## Non-goals (for now)

- Score or stats on the game over screen
- Retry-in-place without scene reload
