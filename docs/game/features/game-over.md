# Game over

All human players dead. Related: [health.md](health.md), [scenarios.md](scenarios.md), [lobby.md](lobby.md).

## Requirements

- When **every local human player** has died (pawn removed or health ≤ 0), the game enters **game over**.
- Gameplay pauses and a **Game Over** overlay appears with a **Continue** button.
- **Continue** navigation depends on how the world scene was entered:
  - From the **lobby** → return to `lobby.tscn`
  - **Direct world load** (solo default keyboard / automation) → reload `world.tscn` for a fresh session (same scenario path is preserved)
- Level-end heal/resurrect (between waves levels) is separate from game over; see [scenarios.md](scenarios.md).

## Non-goals (for now)

- Score or stats on the game over screen
- Retry-in-place without scene reload
