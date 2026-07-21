# Local input (technical)

Implements [../../game/features/local-input.md](../../game/features/local-input.md). Related: [controllers.md](controllers.md), [local-play-context.md](local-play-context.md), [lobby.md](lobby.md).

## Requirements

- **`InputDeviceId`** (`Minimap.Client.LocalPlay`): `Keyboard` or `Joypad(int deviceIndex)`.
- **`LocalPlayRoster`**: ordered local players (1–4), each with a **set** of `InputDeviceId` (one-to-many).
- **`LocalPlayContextNode`** (autoload): holds roster across scene changes; `Clear()` on lobby enter; `ApplyDefaultSoloKeyboard()` when `WorldApp` loads with empty roster.
- **`LocalInputAggregator`** (`Minimap.Client`):
  - per-player merged **move** axis from all bound devices; keyboard **WASD** via `WorldView` held keys when Keyboard is in the set; joypad left stick + D-pad via `Input` APIs.
  - per-player merged **aim** axis from all bound devices; keyboard **arrow keys** via `WorldView`; joypad right stick via `Input` APIs.
  - same deadzone / clamp for move and aim.
- **`ReconnectOverlay`**: shown while **`WorldApp` gameplay is paused** (simulation tick skipped; scene tree keeps processing for UI/input). Listens `Input.JoyConnectionChanged`; drop via `ClientSession.DropHumanPlayer` → Simulation `GameSession.DropHumanPlayer`.
- **Automation** (`IPlaybookContext`): lobby snapshot, activate/back keys, injected joypad buttons; helpers in `Minimap.Automation`.

## Non-goals (for now)

- InputMap action layer (direct key/button reads for now)
