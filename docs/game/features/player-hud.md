# Player HUD

On-screen player status for local humans. Related: [health.md](health.md), [resources.md](resources.md), [ui-icons.md](ui-icons.md).

## Requirements

- Support **1 to 4** player HUD slots.
- Slots are arranged in a **panel along the bottom** of the screen.
- Each slot shows:
  - **Player name** (currently hardcoded as `"Player 1"`, `"Player 2"`, … — not bound to simulation identity data yet)
  - A **horizontal list** of **visible** character resources (see [resources.md](resources.md)), sorted by **uiPriority** (higher first), each with **icon** and amount
  - Resources with a limit show **`VALUE / MAX`** (limit types themselves are not listed)
- When a pawn is missing or dead, health displays as **0 / max** (max may still reflect the last known max, or 0 if never possessed).
- If more than **4** visible resource rows would show, truncate and append a **`+N`** overflow label.

## Non-goals (for now)

- Binding display names to simulation/player profile data
- Health bars as graphical meters (text amounts are enough)
- Death animations or HUD flourish on death
- Tabbed resource UI (a truncated list is enough)
