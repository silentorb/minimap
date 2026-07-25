# Error handling

Cross-cutting standards for product and agent-authored code. Feature docs may still define specific fail-fast or abort contracts (e.g. [lobby.md](../ui/lobby.md), [core-settings.md](core-settings.md), [extensions.md](extensions.md)); this file is the shared policy for **how** to choose and wire failures.

## Principles

1. Prefer **explicit outcomes** (`Try*`, local Ok/Error result types, validated returns) for **expected** failure modes callers can or must handle.
2. Use **exceptions** for **truly exceptional** cases: programmer invariants (misused APIs), corrupted impossible state, or a documented **fail-fast abort** boundary where recovery is not defined (e.g. lobby / world boot).
3. Do **not** swallow failures (empty `catch`, silent defaults that look like success).
4. Do **not** leave half-initialized interactive state when multi-step setup fails (see lobby / world boot abort).
5. For any path longer than a line or two: **choose and document** the failure mode (throw, return outcome, abort UI, soft no-op) before implementing; match the layer guidance below.

Do **not** introduce a shared `Result<T>` library unless a later design change explicitly adds one. When adding explicit flows, prefer local Ok/Error types or `Try*` APIs consistent with `PlaybookResult` / playbook load outcomes.

## Layer guidance

| Layer | Guidance |
|-------|----------|
| **Simulation / Types / Extensive** | Throw on API misuse and broken invariants. Prefer `Try*` / nullable for expected misses (lookups, collision). No Godot logging. |
| **App load / boot** | Fail-fast remains valid for missing/invalid config and extensions when the product contract is abort. Prefer a **single catch boundary** that surfaces (`GD.PushError`) and exits rather than scattering uncaught throws across `_Ready`. Lobby (`LobbySceneBoot` / `LobbyApp`) and world (`WorldSceneBoot` / `WorldApp`) both abort, push error, and quit(1) on boot failure. Scene changes via `ChangeSceneToFile` must check `Error.Ok` and surface failure (push error + throw)—do not ignore the return code. |
| **Client gameplay** | Soft early-return / null for missing **optional** nodes is fine. Required scene wiring should fail loudly at attach time, not mid-frame. Lookups such as `TryGetPlayerPosition` return null on miss—no silent fallback to another entity. |
| **Automation / gRPC** | Keep Ok/Error on the wire for business failures. Do not invent a parallel exception API for playbook/RPC outcomes. |

## Layer contrast (throw vs Ok/Error)

- **App** extension and settings load **throw** into the lobby/world abort boundary (product fail-fast). `ExtensionLoader.LoadedExtensions` is a **success payload only**, not an Ok/Error result.
- **Automation** playbook library load returns **Ok/Error** (`PlaybookRegistry.LoadOutcome`, gRPC responses) so the wire protocol can report failure without aborting the Godot process.

## World-state snapshot limitation

Automation world snapshots / `GetWorldState` may report `player0_x` / `player0_y` as `0,0` when the player position is absent (proto fields are non-optional floats). Callers must **not** treat zeros alone as proof of presence—use layer child counts, human player count, or `TryGetPlayerPosition` nullability.

## Agent checklist

When adding or editing multi-step logic:

1. List failure points on the path.
2. Classify each as **expected** (explicit outcome) vs **exceptional / abort** (throw or single catch boundary).
3. Ensure callers (or the abort boundary) **see** the failure—no silent success lookalikes.
4. Cover the documented failure contract with tests when sound (see [testing.md](testing.md)).
5. Update the relevant feature doc if failure behavior is product-visible.
