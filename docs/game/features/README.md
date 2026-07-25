# Game feature docs (read on demand)

**Features index** for CompuQuest (the surface game on Minimap). **Source of truth** for design, feel, and player-facing rules beyond the high-level pillars in [game-design.md](../game-design.md). **Do not** open every file for general engineering tasks.

Feature docs are grouped under `ui/`, `gameplay/`, `session/`, and `platform/` (further nesting may be added later).

1. Skim the **trigger** lines below.
2. If a trigger matches your current task, read **only** that markdown file.

| File | Read when… |
|------|------------|
| [../game-design.md](../game-design.md) | Reading **gameplay vision**, genre pillars, co-op scope, or world/evolution feel. **Do not edit** unless the user explicitly instructed changes to that file. |
| [gameplay/resources.md](gameplay/resources.md) | Changing **actor/character resources**, resource types, limits, HUD resource lists, or effect resource cost/grant. |
| [gameplay/health.md](gameplay/health.md) | Changing **hit points**, max health, **indestructible** actors, or **death / removal** on zero health. |
| [gameplay/hunger.md](gameplay/hunger.md) | Changing **energy** / max energy, energy drain, vitality bands, **Eat**, or food-gated ability enable. |
| [gameplay/movement.md](gameplay/movement.md) | Changing **realtime cartesian movement**, screen-axis move input, or wall/character **slide** collision. |
| [gameplay/damage.md](gameplay/damage.md) | Changing how **damage** applies, missile / Swing damage amounts, placeable hits, or friendly-fire rules. |
| [gameplay/depiction.md](gameplay/depiction.md) | Changing **depiction** on character/accessory/actor definitions, Kenney / SpriteFrames presentation, or how pawns look. |
| [ui/ui-icons.md](ui/ui-icons.md) | Changing **UI / data-record icons**, game-icons art under `assets/compuquest/game-icons/`, or definition `icon` fields. |
| [gameplay/actors.md](gameplay/actors.md) | Changing **actors**, actor definitions, cell-anchored actors, or the actor vs character split. |
| [gameplay/characters.md](gameplay/characters.md) | Changing **character definitions**, CompuQuest `config/characters/` JSON, default character, or definition vs instance rules. |
| [gameplay/accessories.md](gameplay/accessories.md) | Changing **accessories**, **abilities**, accessory effects, Gun, Farm, Geek, CompuQuest `config/accessories/` JSON, or effect-cache rules. |
| [gameplay/active-abilities.md](gameplay/active-abilities.md) | Changing **dedicated vs modal** ability activation, preview/confirm vs immediate activate, D-pad / 1–4 select, or ability activate binds. |
| [gameplay/interaction.md](gameplay/interaction.md) | Changing **environment interact**, target highlight, A/E binds, or ability interaction effects. |
| [gameplay/farming.md](gameplay/farming.md) | Changing **Farm**, grow/harvest, seedlings, food yields, or vegetable actors. |
| [gameplay/cell-placement.md](gameplay/cell-placement.md) | Changing **cell occupancy**, placing actors on cells, placement preview, or Farm/Geek place rules. |
| [gameplay/tags.md](gameplay/tags.md) | Changing **tags** on definitions (e.g. `player_selectable`), resource type ids as tags, or tag naming policy. |
| [gameplay/domains.md](gameplay/domains.md) | Changing **domains** (gardening / computing themes), domain tags on accessories, or domain-colored icon swatches. |
| [session/players.md](session/players.md) | Changing **player** records, accessory points, or lobby→spawn accessory choices. |
| [gameplay/ai.md](gameplay/ai.md) | Changing **AI** wander / grass goals, AI combat behavior, or AI spawn counts per faction. |
| [gameplay/combat.md](gameplay/combat.md) | Changing **missiles**, **Swing**, shoot/swing effects, fire rate, primary/secondary fire, mouse/stick aim, or AI nearest-hostile aim. |
| [gameplay/factions.md](gameplay/factions.md) | Changing **faction** membership, hostility, or surface faction ids / spawn mix. |
| [session/map-layout.md](session/map-layout.md) | Changing **map shape or size** (rectangle extents, single-screen arena). |
| [ui/player-hud.md](ui/player-hud.md) | Changing **player HUD** slots, names, or on-screen resource / health display. |
| [ui/lobby.md](ui/lobby.md) | Changing **local player lobby**, join panels, or lobby → world start flow. |
| [ui/main-menu.md](ui/main-menu.md) | Changing **main menu** screen or popup, start screen, New/Quit/Continue, or pause overlay. |
| [session/scenarios.md](session/scenarios.md) | Changing **scenario** JSON, wave pacing, spawner counts, level transitions, or single-map sandbox session rules. |
| [gameplay/waves.md](gameplay/waves.md) | Changing **wave timing**, spawner placement, per-wave enemy volume, or the parked/disabled wave stance. |
| [ui/game-over.md](ui/game-over.md) | Changing **game over** detection, overlay, or post-game navigation. |
| [session/local-input.md](session/local-input.md) | Changing **gamepad/keyboard** binding, per-player devices, disconnect/reconnect, or in-world input. |
