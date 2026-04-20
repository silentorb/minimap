# Remote headless Godot (WSL launcher)

To run **headless Windows Godot** from this dev container, use the narrow HTTP launcher in [`tools/godot-launcher/`](../../../tools/godot-launcher/). **Build it** with [`scripts/build_godot_launcher.sh`](../../../scripts/build_godot_launcher.sh) from the repo root (requires Rust in the dev container; see [`.devcontainer/Dockerfile`](../../../.devcontainer/Dockerfile)). You still typically **run** `dist/tools/godot-launcher` in **WSL** so it can spawn Windows `Godot.exe`. Agents call [`scripts/godot_remote.py`](../../../scripts/godot_remote.py) (`python3`).

## Dev container defaults

[`.devcontainer/devcontainer.json`](../../../.devcontainer/devcontainer.json) sets:

- **`GODOT_REMOTE_URL=http://127.0.0.1:27182`** — loopback matches the launcher default; **`runArgs` includes `--network=host`** so the container can reach the WSL host’s listener when that is how your setup routes traffic.

Optional overrides: [`scripts/godot_remote.env.example`](../../../scripts/godot_remote.env.example).

## WSL side (launcher)

Configure and start `godot-launcher` using [`tools/godot-launcher/README.md`](../../../tools/godot-launcher/README.md) and [`tools/godot-launcher/.env.example`](../../../tools/godot-launcher/.env.example). Minimum: **`GODOT_LAUNCHER_GODOT_EXE`**. If the project path inside the container is under `/workspaces/...`, set **`GODOT_LAUNCHER_WORKSPACE_LINUX`** and **`GODOT_LAUNCHER_WORKSPACE_WINDOWS`** so the launcher maps to a Windows `--path` Godot understands.

Smoke-check from inside the container:

```bash
python3 scripts/godot_remote.py health
```

## xUnit + gRPC Godot functional tests (`Minimap.Functional.Godot.Tests`)

Godot functional tests still need **`GODOT_BIN`** pointing at a Godot executable the test process can spawn. The HTTP launcher does not replace `GODOT_BIN`; it complements it (health checks, manual `/sessions`, etc.).

Typical setups:

- **WSL (not restricted to a Linux-only container)** — export Windows Godot, then run tests:

  ```bash
  export GODOT_BIN=/mnt/c/Apps/Godot/Godot_v4.6-stable_win64.exe   # your path
  ./scripts/run_godot_functional_tests.sh
  ```

  `run_godot_functional_tests.sh` runs `python3 scripts/godot_remote.py health` when **`GODOT_REMOTE_URL`** is set, then runs `dotnet test` for `Minimap.Functional.Godot.Tests`.

- **Pure Linux environment without Windows Godot** — install a Linux Godot 4.x build and set **`GODOT_BIN`** to that binary, or run the Godot functional project on a machine that has Godot.

The xUnit fixture starts Godot with automation env vars (`MINIMAP_AUTOMATION_ENABLED`, `MINIMAP_AUTOMATION_HOST`, `MINIMAP_AUTOMATION_PORT`) and drives gameplay checks via protobuf gRPC.

See [`tools/godot-launcher/README.md`](../../../tools/godot-launcher/README.md) for API and security notes.
