# Player lobby

Local multiplayer join screen before a match. Related: [local-input.md](../session/local-input.md), [player-hud.md](player-hud.md), [players.md](../session/players.md), [user-profiles.md](../session/user-profiles.md), [accessories.md](../gameplay/accessories.md), [tags.md](../gameplay/tags.md), [main-menu.md](main-menu.md).

## Requirements

- **Local multiplayer only** — no networking or remote join.
- The lobby is reached via **New** on the [main menu](main-menu.md) (or `START_SCREEN=lobby` for developers), or by finishing a match on the [post-session summary](post-session.md).
- The screen is divided into **four horizontal panels**, one per possible player (1–4).
- Each panel is a **wizard** with modes in sequence:
  1. **Available** (inactive) — default; no player assigned.
  2. **Selecting profile** — device has joined; **profile carousel** (select-only) is shown. Left/right cycle available [user profiles](../session/user-profiles.md); **Enter** / **Start** confirm and advance. Create/rename/delete are **not** in the lobby (use main menu **Profiles**). A profile may be chosen by at most one slot. If none are available, show that profiles are managed from the Profiles screen; cannot advance.
  3. **Selecting accessories** — **accessory selection panel** is shown and interactive.
  4. **Ready** — ready to play; customize area hidden but **chosen profile, accessories, and remaining points persist**.
- Each panel shows **Back** / **Forward** buttons at the bottom when the current step can navigate earlier or later in the wizard (same transitions as Escape/B and Enter/Start). Available has neither; Ready has Back only; Forward is disabled when profile confirm is impossible.
- Accessory selection UI (Selecting accessories only) fills the player panel’s customize area and scales to that parent (no clipping the main picker). A thin **points available** line sits above three equal-height child panels that share the remaining vertical space; each scrolls **vertically** when its content overflows:
  1. **Available** — icon grid of unowned selectable accessories.
  2. **Description** — name, cost, and body of the focused accessory.
  3. **Owned** — prior-stage locked accessories plus this-stage choices.
  Activating an available accessory spends points and moves it to Owned; activating a this-stage owned accessory returns it to Available and refunds points. Accessories acquired in a **previous stage** stay in Owned and cannot be unchosen. Icon grids scale to the panel width (no horizontal scroll). Budget comes from core **`player.accessoryPoints`** (default 2). Catalog comes from the integrator’s player-selectable accessories. The selection UI must remain **inside its player panel** and **inside the lobby viewport**.
- Input while Selecting accessories: arrows / D-pad / stick navigate grids; **Space** / **A** take or return the focused accessory (if allowed); **Enter** / **Start** ready up; **Escape** / **B** back to profile selection (keeps accessory choices).
- Input while Selecting profile: ←/→ cycle; **Enter** / **Start** confirm profile; **Escape** / **B** back to Available (clears devices and selection).
- Ready + **Escape** / **B** → Selecting accessories (keeps choices).
- Players need not spend all accessory points to ready.
- **Claim**: an unassigned input device presses **activate** (`JoyButton.A` or `JoyButton.Start`, or keyboard Enter/Space) → claims the **lowest-index Available** panel and binds that device to the slot (enters Selecting profile).
- **Start game**: when **at least one** panel is claimed and **every claimed** panel is Ready (Available panels ignored) → navigate to the world scene with that player count, device bindings, selected profiles, and selected accessories.
- A device already bound to a slot cannot claim another (except the reconnect flow in [local-input.md](../session/local-input.md)).
- **Keyboard** may claim **one** lobby slot like a gamepad.
- **Return from a match**: when arriving from post-session (or other world→lobby end paths that keep the roster), the lobby **restores** still-connected players with their previous profile and accessory selections in **Selecting accessories** (one step before Ready). Disconnected joypad players are not restored. Fresh entry from the main menu still starts empty.
- **Leave to main menu**: the **primary** player (lowest-index claimed slot; if none claimed, any unbound Back/Escape) may return to the [main menu](main-menu.md). With claimed slots, the primary backs through the wizard to Available (unclaim), then Back/Escape leaves to the main menu and clears the play context. This is how players reach the main menu after a match has started.

## Non-goals (for now)

- Online or remote lobby
- Changing panel count at runtime
- Profile create/rename/delete inside the lobby

## Developer / test entry

Loading `res://scenes/world.tscn` directly (without lobby) still starts a game for developers and automated tests; see [local-input.md](../session/local-input.md). Direct world entry creates players with default accessory points, **no** selected accessories, and **no** user profile.
