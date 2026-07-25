# Player HUD

On-screen player status for local humans. Related: [health.md](../gameplay/health.md), [resources.md](../gameplay/resources.md), [ui-icons.md](ui-icons.md), [active-abilities.md](../gameplay/active-abilities.md), [hunger.md](../gameplay/hunger.md).

## Requirements

- Support **1 to 4** player HUD slots.
- Slots are arranged in a **panel along the bottom** of the screen.
- Each slot shows:
  - **Player name** — [user profile](../session/user-profiles.md) display name when the player entered from the lobby with a profile; otherwise `"Player 1"`, `"Player 2"`, …
  - A **horizontal list** of **visible** character resources (see [resources.md](../gameplay/resources.md)), sorted by **uiPriority** (higher first), each with **icon** and amount
  - Resources with a limit show **`VALUE / MAX`** (limit types themselves are not listed)
  - The **currently selected modal ability** (icon + display name), when one is selected; hidden when none (disabled abilities are already omitted from the loadout)
- When a pawn is missing or dead, health displays as **0 / max** (max may still reflect the last known max, or 0 if never possessed).
- If more than **4** visible resource rows would show, truncate and append a **`+N`** overflow label.

## Non-goals (for now)

- Health bars as graphical meters (text amounts are enough)
- Death animations or HUD flourish on death
- Tabbed resource UI (a truncated list is enough)
- Full ability slot strip / cooldown chrome beyond the selected modal ability
