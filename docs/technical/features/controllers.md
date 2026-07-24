# Controllers

Unreal-style controller / pawn separation. Implements game [ai.md](../../game/features/ai.md) and [combat.md](../../game/features/combat.md) control paths. Related: [characters-and-factions.md](characters-and-factions.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [player-hud.md](player-hud.md), [local-input.md](local-input.md), [navigation.md](navigation.md).

## Requirements

- Characters (pawns) do not own input devices or AI brains. An **`IController`** attaches to a character and drives it.
- **`IController`** (Simulation):
  - `Possess(Character)` / `Unpossess()`
  - `Pawn` property (possessed character or null)
  - `Tick(GameWorld, float dt)` — writes move/fire intents for the pawn
- Implementations:
  - **`PlayerController` (Minimap.Client)**: constructed with a Simulation **`Player`**; receives move/aim, fire held, modal select, ability activate/back; updates facing via move intent; calls shared shoot with `wantsFire`; runs modal placement preview/confirm. **Not** part of Simulation (Simulation has no user input APIs).
  - **`AiController` (Simulation)**: periodically picks a random grass-hex world goal (or pause); asks an **`IMoveSteering`** for move intent toward that goal; supplies fire direction toward the nearest living hostile (or zero if none) and sets `wantsFire` when aim is non-zero. Default steering is headless **`DirectMoveSteering`**. Godot play upgrades steering via [navigation.md](navigation.md).
- **`IMoveSteering`** (Simulation): `SetGoal` / `ClearGoal` / `SampleMoveIntent(Character, dt)`; optional dispose when replaced. Controllers own goals; steering only converts goal → direction.
- **Shared shoot** helper (Simulation): reads the first **`IShootEffect`** on **`character.Effects`**; ticks that effect’s cooldown; when ready, `wantsFire` is true, and aim (else facing) is non-zero, spawns a missile. Controllers choose direction and whether fire is requested; they do **not** own fire cooldown. Concrete `ShootEffect` lives in CompuQuest. Nearest-hostile lookup lives on the helper for AI (and tests); no hard-coded faction ids.
- **Minimap.Simulation** owns `GameSession` (world create, scenario tick, **Players**, game-over). **Minimap.Client** (`ClientSession` / `WorldApp`) attaches `PlayerController`s to each session `Player`’s character, feeds per-player input via **`LocalInputAggregator`**, and wires Godot navigation steering for AI (see [navigation.md](navigation.md)).
- **`GameWorld.Tick(dt)` order**: controllers → apply movement (updates **Facing**) → tick missiles → apply damage / remove dead → prune missiles.

## Non-goals (for now)

- Networked remote controllers
