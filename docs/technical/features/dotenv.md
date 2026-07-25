# Dotenv (manual play)

Optional repo-root `.env` loading for **manual Godot play** development overrides. Owned by **Minimap.App** (`DotEnvBootstrap`).

## Requirements

- Package: **DotNetEnv** (process environment from `.env`).
- File location: `.env` next to `project.godot` (see `.env.example`).
- Load timing: early host composition (`AppHostRegistration` ModuleInitializer) before lobby/world `_Ready`.
- Missing `.env`: soft no-op.
- Malformed `.env`: fail-fast (throw).
- **No clobber:** variables already set in the process environment win over `.env`.
- **Never** apply during unit or functional tests (see gates below).

## Gates (must all pass)

`DotEnvBootstrap.ShouldApply` returns false when any of:

| Gate | Signal |
|------|--------|
| Explicit disable | `MINIMAP_DOTENV_DISABLED` truthy (`1` / `true` / `yes`) |
| Automation SUT | `MINIMAP_AUTOMATION_ENABLED` truthy |
| Test host | Entry assembly is `testhost` or ends with `.Tests` |

Godot functional tests also set `MINIMAP_DOTENV_DISABLED=1` on the child process.

## Bootstrap env knobs

After dotenv (or a real shell export), Client resolves via App-registered `WorldHostHooks`:

| Variable | Effect | Precedence |
|----------|--------|------------|
| `MINIMAP_SCENARIO` | Scenario path for world boot | CLI `--scenario` > env > play-context / scene default |
| `MINIMAP_WORLD_SEED` | World seed (int) | Env (when parseable) > `WorldApp.WorldSeed` export |

Definition JSON under `config/` and extension `config/` remains file-edited; `.env` is for process overrides, not a second JSON schema.

## Non-goals

- Hot-reload of `.env` during play
- Shipping a committed `.env` (gitignored; use `.env.example`)
- Replacing core/scenario/definition JSON loaders
