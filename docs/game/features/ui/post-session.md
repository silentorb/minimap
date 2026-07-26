# Post-session summary

Full-screen summary after a local match ends. Related: [game-over.md](game-over.md), [lobby.md](lobby.md), [main-menu.md](main-menu.md), [../session/achievements.md](../session/achievements.md), [../session/user-profiles.md](../session/user-profiles.md).

## Requirements

- When a session **finishes**, gameplay pauses and a **post-session summary** fills the screen. This replaces the old game-over menu (New game / Main menu).
- Session finish paths:
  1. **Game over** — every local human player is dead.
  2. **End game** — a player chooses **End game** from the in-world pause menu.
- The summary shows **one section per local player** (profile display name when present). For each player:
  - **Achievements acquired this session** — list every achievement earned during the session, even if that profile already had it unlocked. Mark **first-time** unlocks distinctly from re-earns.
  - No other stats in the first cut (do **not** show death counts here).
- Like the lobby, players enter a **Ready** state on this screen. Navigation away does **not** happen until **every** local player still in the session is Ready.
- When all are Ready, navigate to the [lobby](lobby.md). The lobby **restores** prior profile and accessory selections for still-connected players, one step before Ready (**Selecting accessories**). See lobby docs.
- There is **no** Main menu control on the post-session screen. After a session has started, leaving toward the main menu always passes through the lobby (primary player backs out from there).

## Non-goals (for now)

- Score, kills, survival time, or death stats on this screen
- Retry-in-place without returning to the lobby
- Skipping the summary on End game
