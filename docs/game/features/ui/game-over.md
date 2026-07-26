# Game over

All human players dead. Related: [post-session.md](post-session.md), [../gameplay/health.md](../gameplay/health.md), [../session/scenarios.md](../session/scenarios.md), [lobby.md](lobby.md), [main-menu.md](main-menu.md).

## Requirements

- When **every local human player** has died (pawn removed or health ≤ 0), the game enters **game over**.
- Gameplay pauses and the [post-session summary](post-session.md) appears (not a New game / Main menu dialog).
- Level-end heal/resurrect (between waves levels) is separate from game over; see [scenarios.md](../session/scenarios.md).

## Non-goals (for now)

- Retry-in-place without returning to the lobby
