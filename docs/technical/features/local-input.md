# Local input (technical)

Implements [../../game/features/local-input.md](../../game/features/local-input.md). Related: [controllers.md](controllers.md), [active-abilities.md](active-abilities.md), [local-play-context.md](local-play-context.md), [lobby.md](lobby.md).

## Requirements

- **`InputDeviceId`** (`Minimap.Client.LocalPlay`): `Keyboard` or `Joypad(int deviceIndex)`.
- **`LocalPlayRoster`**: ordered local players (1–4), each with a **set** of `InputDeviceId` (one-to-many).
- **`LocalPlayContextNode`** (autoload): holds roster across scene changes; `Clear()` on lobby enter; `ApplyDefaultSoloKeyboard()` when `WorldApp` loads with empty roster.
- **`LocalInputAggregator`** (`Minimap.Client`):
  - per-player **`PlayerWorldInput`**: move, aim, fire held, ability activate/back/interact edges, optional modal select index.
  - move: keyboard **WASD**; joypad **left stick only** (no D-pad move).
  - aim: keyboard **mouse** via `WorldView.ReadMouseAimFrom`; joypad right stick.
  - fire: Right Trigger / LMB.
  - modal select: D-pad / keys 1–4 (edge per slot).
  - activate: `JoyButton.X` / Space (edge); back: `JoyButton.B` / Escape (edge).
  - environment interact: `JoyButton.A` / **E** (edge); see [interaction.md](interaction.md).
  - same deadzone / clamp for move and aim axes.
- **`ReconnectOverlay`**: shown while **`WorldApp` gameplay is paused** (simulation tick skipped; scene tree keeps processing for UI/input). Listens `Input.JoyConnectionChanged`; drop via `ClientSession.DropHumanPlayer` → Simulation `GameSession.DropHumanPlayer`.
- **Automation** (`IPlaybookContext`): lobby snapshot, activate/back keys, injected joypad buttons; helpers in `Minimap.Automation`.

## Non-goals (for now)

- InputMap action layer (direct key/button reads for now)
