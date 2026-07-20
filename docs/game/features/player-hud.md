# Player HUD

On-screen player status for local humans. Related: [health.md](health.md).

## Requirements

- Support **1 to 4** player HUD slots.
- Slots are arranged in a **panel along the bottom** of the screen.
- Each slot shows:
  - **Player name** (currently hardcoded as `"Player 1"`, `"Player 2"`, … — not bound to simulation identity data yet)
  - **Current health / max health** from the possessed character
- When a pawn is missing or dead, health displays as **0** (max may still reflect the last known max, or 0 if never possessed).

## Non-goals (for now)

- Binding display names to simulation/player profile data
- Health bars as graphical meters (text `current/max` is enough)
- Death animations or HUD flourish on death
