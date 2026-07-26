# Local input (technical)

Implements [local-input.md](../../../game/features/session/local-input.md). Related: [controllers.md](../gameplay/controllers.md), [active-abilities.md](../gameplay/active-abilities.md), [local-play-context.md](local-play-context.md), [lobby.md](../ui/lobby.md), [main-menu.md](../ui/main-menu.md).

## Requirements

- **`InputDeviceId`** (`Minimap.Client.LocalPlay`): `Keyboard` or `Joypad(int deviceIndex)`.
- **`LocalPlayRoster`**: ordered local players (1–4), each with a **set** of `InputDeviceId` (one-to-many).
- **`LocalPlayContextNode`** (autoload): holds roster across scene changes; `Clear()` on lobby enter; `ApplyDefaultSoloKeyboard()` when `WorldApp` loads with empty roster.
- **`LocalInputAggregator`** (`Minimap.Client`):
  - per-player **`PlayerWorldInput`**: move, aim, primary/secondary fire held, ability activate/back/interact edges, optional modal cycle delta (`-1` / `+1`).
  - move: keyboard **WASD**; joypad **left stick only** (no D-pad move).
  - aim: keyboard **mouse** via `WorldView.ReadMouseAimFrom`; joypad right stick.
  - primary fire: Right Trigger / LMB.
  - secondary fire: Left Trigger / RMB.

  - modal cycle: D-pad Left/Right / `[`/`]` (press edge per direction; both directions same frame → no cycle).
  - activate: `JoyButton.X` / Space (edge); back: `JoyButton.B` / Escape (edge).
  - environment interact: `JoyButton.A` / **E** (edge); see [interaction.md](../gameplay/interaction.md).
  - same deadzone / clamp for move and aim axes.
- **`ReconnectOverlay`**: shown while **`WorldApp` gameplay is paused** (simulation tick skipped; scene tree keeps processing for UI/input). Listens `Input.JoyConnectionChanged`; drop via `ClientSession.DropHumanPlayer` → Simulation `GameSession.DropHumanPlayer`.
- **Main menu popup**: Start / Escape open while not in reconnect/game-over/main-menu modal; Escape prefers ability/placement cancel when that edge applies. Pause + exclusive owner device — see [main-menu.md](../ui/main-menu.md).
- **Automation** (`IPlaybookContext`): lobby snapshot, activate/back keys, injected joypad buttons; helpers in `Minimap.Automation`.

## Non-goals (for now)

- InputMap action layer (direct key/button reads for now)
