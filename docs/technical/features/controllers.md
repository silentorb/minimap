# Controllers

Unreal-style controller / pawn separation in the simulation. Implements game [ai.md](../../game/features/ai.md) and [combat.md](../../game/features/combat.md) control paths. Related: [characters-and-factions.md](characters-and-factions.md).

## Requirements

- Characters (pawns) do not own input devices or AI brains. An **`IController`** attaches to a character and drives it.
- **`IController`**:
  - `Possess(Character)` / `Unpossess()`
  - `Pawn` property (possessed character or null)
  - `Tick(GameWorld, float dt)` — writes move/fire intents for the pawn
- Implementations:
  - **`PlayerController`**: receives move axes from the client via `SetMoveInput(SimVec2)`; each tick applies move intent and shared autoshoot.
  - **`AiController`**: picks random wander directions periodically; same shared autoshoot as the player.
- **Shared autoshoot** helper: nearest living hostile by faction rules; fire on cooldown. No hard-coded faction ids.
- Client (`WorldRoot`) only feeds keyboard move input into the human `PlayerController`; it does not call `SetPlayerInput` on indices directly.
- **`GameWorld.Tick(dt)` order**: controllers → apply movement → tick missiles → apply damage / remove dead → prune missiles.

## Non-goals (for now)

- Multiple local human controllers (1–4 co-op can return later)
- Networked remote controllers
