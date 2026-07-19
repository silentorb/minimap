# Controllers

Unreal-style controller / pawn separation. Implements game [ai.md](../../game/features/ai.md) and [combat.md](../../game/features/combat.md) control paths. Related: [characters-and-factions.md](characters-and-factions.md), [player-hud.md](player-hud.md).

## Requirements

- Characters (pawns) do not own input devices or AI brains. An **`IController`** attaches to a character and drives it.
- **`IController`** (Simulation):
  - `Possess(Character)` / `Unpossess()`
  - `Pawn` property (possessed character or null)
  - `Tick(GameWorld, float dt)` — writes move/fire intents for the pawn
- Implementations:
  - **`PlayerController` (Minimap.Client)**: receives move axes via `SetMoveInput(SimVec2)`; each tick applies move intent and shared autoshoot. **Not** part of Simulation (Simulation has no user input APIs).
  - **`AiController` (Simulation)**: picks random wander directions periodically; same shared autoshoot as the player.
- **Shared autoshoot** helper (Simulation): nearest living hostile by faction rules; fire on cooldown. No hard-coded faction ids.
- **Minimap.App** creates the world, attaches Client `PlayerController`s to unpossessed human pawns, and feeds keyboard move input into local player index 0.
- **`GameWorld.Tick(dt)` order**: controllers → apply movement → tick missiles → apply damage / remove dead → prune missiles.

## Non-goals (for now)

- Extra gamepads / per-slot local input devices (slots 1–3 receive zero move input)
- Networked remote controllers
