# Main menu

Title / pause menu for local play. Related: [lobby.md](lobby.md), [game-over.md](game-over.md), [post-session.md](post-session.md), [../session/local-input.md](../session/local-input.md), [../session/user-profiles.md](../session/user-profiles.md), [../session/achievements.md](../session/achievements.md).

## Requirements

- There are two presentations of the same menu:
  1. **Main menu screen** (also the **start screen** for now) — full black background with title text **CompuQuest Mini** and options **New**, **Profiles**, and **Quit**.
  2. **Main menu popup** — opened during an active match; dimmed black overlay over the paused game with options **Continue**, **End game**, and **Quit**. No **Profiles** on the popup.
- **New** (screen only) starts a new local session by going to the **lobby**.
- **End game** (popup only) ends the current match and opens the [post-session summary](post-session.md). It does **not** go straight to the lobby or main menu.
- **Profiles** (screen only) opens the [user profiles](../session/user-profiles.md) management screen.
- **Quit** exits the application.
- **Continue** (popup only) dismisses the popup and resumes gameplay.
- While the popup is open, gameplay is **paused** and the background is partially blacked out (semi-transparent black overlay).
- Players open the popup during play with **Start** or **Escape** (see [local-input.md](../session/local-input.md) for cancel-vs-menu precedence).
- Whichever player activates the popup has **exclusive control** over it until it closes; other players’ input is ignored for the menu.
- The owning device navigates options with **D-pad Up/Down** (gamepad) or **Up/Down** (keyboard), activates with **A** / **Enter** / **Space**, and dismisses with **Start** / **Escape** (**Continue**).
- Once a match has started, there is **no** direct navigation from the world to the main menu screen; players return via post-session → lobby, then the [primary player may leave the lobby](lobby.md) to the main menu.

## Non-goals (for now)

- A separate splash / pre-title screen before the main menu
- Settings, credits, or multiplayer online options (beyond **Profiles**)
- Saving / loading mid-match beyond Continue
- Profiles management from the in-world popup
