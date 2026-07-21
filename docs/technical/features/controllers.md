# Controllers

Unreal-style controller / pawn separation. Implements game [ai.md](../../game/features/ai.md) and [combat.md](../../game/features/combat.md) control paths. Related: [characters-and-factions.md](characters-and-factions.md), [accessories.md](accessories.md), [player-hud.md](player-hud.md), [local-input.md](local-input.md).

## Requirements

- Characters (pawns) do not own input devices or AI brains. An **`IController`** attaches to a character and drives it.
- **`IController`** (Simulation):
  - `Possess(Character)` / `Unpossess()`
  - `Pawn` property (possessed character or null)
  - `Tick(GameWorld, float dt)` — writes move/fire intents for the pawn
- Implementations:
  - **`PlayerController` (Minimap.Client)**: receives move axes via `SetMoveInput(SimVec2)` and aim axes via `SetAimInput(SimVec2)`; each tick applies move intent and calls shared shoot with the aim direction (zero aim = no fire). **Not** part of Simulation (Simulation has no user input APIs).
  - **`AiController` (Simulation)**: picks random wander directions periodically; supplies fire direction toward the nearest living hostile (or zero if none).
- **Shared shoot** helper (Simulation): reads the first **`IShootEffect`** on **`character.Effects`**; ticks that effect’s cooldown; when ready and `fireDirection` is non-zero, spawns a missile in that direction. Controllers choose the direction; they do **not** own fire cooldown. Concrete `ShootEffect` lives in CompuQuest. Nearest-hostile lookup lives on the helper for AI (and tests); no hard-coded faction ids.
- **Minimap.App** creates the world, attaches Client `PlayerController`s to unpossessed human pawns, and feeds per-player move and aim input via **`LocalInputAggregator`** from each player’s bound devices (see [local-input.md](local-input.md)).
- **`GameWorld.Tick(dt)` order**: controllers → apply movement → tick missiles → apply damage / remove dead → prune missiles.

## Non-goals (for now)

- Networked remote controllers
