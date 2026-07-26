# Achievements

Per-profile unlocks earned during local play. Related: [user-profiles.md](user-profiles.md), [post-session.md](../ui/post-session.md), [lobby.md](../ui/lobby.md), [health.md](../gameplay/health.md). Technical: [achievements.md](../../../technical/features/session/achievements.md).

## Requirements

- Achievements are tied to an individual **user profile**. Unlock state persists with that profile across app launches.
- From the [Profiles](user-profiles.md) screen, with a profile selected, **Achievements** opens a **full-screen** Achievements view for that profile only. **Back** returns to Profiles (not the main menu). Switching profiles requires returning to Profiles first.
- The Achievements screen lists known achievements with locked / unlocked state for that profile.
- Initial achievement: **Survive 5 minutes** — stay alive for **five consecutive minutes** of simulation time within a **single** session (pause / menu time does not count). If that profile’s character dies, the consecutive timer for that session resets and the achievement is not earned unless five consecutive alive minutes are completed later in another (or the same) session before death.
- When an achievement’s condition is met during a match, it unlocks on the profile immediately (saved). There is no in-match toast for the first cut; the post-session screen and Achievements screen show the result.
- Direct world entry (no lobby / no profile) does not unlock profile achievements.

## Non-goals (for now)

- In-match unlock toasts or celebrations
- Online / cloud achievement sync
- Stats or progress bars beyond locked vs unlocked on the Achievements screen
