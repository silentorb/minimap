# User profiles

Local persistent player identities across app launches. Related: [players.md](players.md), [achievements.md](achievements.md), [lobby.md](../ui/lobby.md), [main-menu.md](../ui/main-menu.md), [player-hud.md](../ui/player-hud.md), [health.md](../gameplay/health.md). Technical: [user-profiles.md](../../../technical/features/session/user-profiles.md).

## Requirements

- Players may create multiple **user profiles**. Each profile has a **display name**, an optional **avatar picture**, persistent stats (initially **death count**), and [achievements](achievements.md).
- **Create, rename, and delete** happen on a dedicated **Profiles** screen reached from the [main menu](../ui/main-menu.md) (**Profiles**), not from the lobby.
- Profiles screen: scrollable **list** of profiles on the left; **detail panel** on the right for the selected profile (avatar preview, name, deaths, rename, delete, **Change picture**, **Clear picture**, **Achievements**). **Create** starts name entry for a new profile. **Back** returns to the main menu. **Achievements** opens the full-screen achievements view for the selected profile.
- **Avatar**: optional local image chosen with a file picker on the Profiles screen (**Change picture**). **Clear picture** removes it. When unset or unloadable, a neutral placeholder is shown. The same avatar appears in the [lobby](../ui/lobby.md) profile carousel / slot title and beside the [player HUD](../ui/player-hud.md) display name.
- Name rules: trimmed, non-empty, max length **24**, **case-insensitive unique** among profiles.
- In the [lobby](../ui/lobby.md), after claiming a slot, each player **selects** a profile (carousel) before accessory selection. A profile may be selected by **at most one** lobby slot at a time.
- If no profiles exist or none remain available for a slot, the lobby shows that profiles are managed from the main menu Profiles screen; the player cannot advance until a free profile is available.
- When a player’s character **dies** during a match (health ≤ 0 / quiet removal), that profile’s death count increments by one and is saved. Disconnect **drop player** does not count as a death.
- Direct world entry (no lobby) has no profile; deaths are not recorded for that path.
- The player HUD shows the profile **display name** (and avatar when set) when the player entered from lobby with a profile; otherwise `"Player N"` with no avatar.

## Non-goals (for now)

- Online accounts or cloud sync
- Profiles entry from the in-world main menu popup
- Stats beyond death count and achievement unlocks
- Avatar editing beyond pick/clear (crop, filters, remote URLs)
