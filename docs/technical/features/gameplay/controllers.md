# Controllers

Unreal-style controller / pawn separation. Implements game [ai.md](../../../game/features/gameplay/ai.md) and [combat.md](../../../game/features/gameplay/combat.md) control paths. Related: [characters-and-factions.md](characters-and-factions.md), [accessories.md](accessories.md), [active-abilities.md](active-abilities.md), [player-hud.md](../ui/player-hud.md), [local-input.md](../session/local-input.md), [navigation.md](navigation.md).

## Requirements

- Characters (pawns) do not own input devices or AI brains. An **`IController`** attaches to a character and drives it.
- **`IController`** (Simulation):
  - `Possess(Character)` / `Unpossess()`
  - `Pawn` property (possessed character or null)
  - `Tick(GameWorld, float dt)` — writes move/fire intents for the pawn
- Implementations:
  - **`PlayerController` (Minimap.Client)**: constructed with a Simulation **`Player`**; receives move/aim, primary/secondary fire held, modal cycle, ability activate/back; updates **facing from non-zero aim** (move never changes facing); calls shared shoot / swing helpers; runs modal placement preview/confirm. **Not** part of Simulation (Simulation has no user input APIs).
  - **`AiController` (Simulation)**: aggression-blended goals (roam or owner anchor vs nearest significant target); timed **injury flee** when health ≤ 30% (chance `1 − aggression`, duration **3s** — toward living owner, else away from nearest hostile); ask an **`IMoveSteering`** for move intent; aim toward the nearest living hostile; updates facing from aim when a hostile exists, else from non-zero move intent; fire `Shoot.Tick` / `Swing.Tick` when in range. Optional `seekCrops` enables farmer harvest/eat. Owned pawns (`Actor.OwnerActorId`) skip roam and stay still within **1 hex** of a living owner unless aggression pulls toward a significant goal. Default steering is headless **`DirectMoveSteering`**. Godot play upgrades steering via [navigation.md](navigation.md). Crazed carrot emerge (`GameWorld.SpawnChaseCharacter`) attaches `AiController` at aggression **0.9**. Shared thresholds live in **`AiTuning`**.
- **`IMoveSteering`** (Simulation): `SetGoal` / `ClearGoal` / `SampleMoveIntent(Character, dt)`; optional dispose when replaced. Controllers own goals; steering only converts goal → direction.
- **Shared shoot** helper (Simulation): Actor-level APIs — first **`IShootEffect`** on effects; ticks cooldown; when ready, `wantsFire` is true, and aim (else facing) is non-zero, spawns a **projectile actor** from a world position + faction (assigns flight state from the effect). Controllers call the Actor convenience overload and choose direction / whether fire is requested; they do **not** own fire cooldown. Cell-actor auto-fire uses the same helper via **`IWorldPassiveEffect`** on CompuQuest `ShootEffect`. Nearest-hostile lookup is by faction + position; projectiles are excluded; no hard-coded faction ids.
- **Shared swing** helper (Simulation): same pattern for **`ISwingEffect`** — cooldown on the effect; when ready and `wantsSwing`, spawn a **`SwingArc`**, resolve half-disk hits once, set cooldown. Concrete `SwingEffect` lives in CompuQuest.
- **Minimap.Simulation** owns `GameSession` (world create, scenario tick, **Players**, game-over). **Minimap.Client** (`ClientSession` / `WorldApp`) attaches `PlayerController`s to each session `Player`’s character, feeds per-player input via **`LocalInputAggregator`**, and wires Godot navigation steering for AI (see [navigation.md](navigation.md)).
- **`GameWorld.Tick(dt)` order**: controllers (may update **Facing**) → apply movement → tick projectiles → tick swing arcs (lifetime) → apply damage / remove dead characters and cell actors.

## Non-goals (for now)

- Networked remote controllers
