# godot-launcher

Small **HTTP/JSON** service intended to run **in WSL** on your Windows machine. It listens on **loopback only**, accepts a **bearer token**, and spawns **Windows Godot** (`/mnt/c/.../Godot*.exe`) in **headless** mode with a fixed `--path` argument—no shell, no arbitrary client commands.

The dev container uses [`scripts/godot_remote.py`](../../scripts/godot_remote.py) (Python stdlib) to call this API.

## Build

### Default (wrapper build from repo root)

This project uses a **small shell wrapper** around Cargo—not a separate “dist” build system. From the **repository root**:

```bash
./scripts/build_godot_launcher.sh
```

It runs **`cargo build --release`** for this crate, then **copies** the release binary into **`dist/tools/`** (gitignored) so there is a stable path outside Cargo’s output tree. In the **dev container**, **`CARGO_TARGET_DIR`** is set to **`/workspaces/build/cargo-target`**; otherwise Cargo writes under **`target/release/`** in this crate.

### In-crate `cargo` (tests and local iteration)

```bash
cd tools/godot-launcher
cargo test
cargo build --release   # under $CARGO_TARGET_DIR/release/ or target/release/
```

Use this when working on Rust code; use the **wrapper script** above when you want the usual **`dist/tools/`** copy as well.

## Configuration (environment variables)

| Variable | Required | Description |
|----------|----------|-------------|
| `GODOT_LAUNCHER_TOKEN` | yes | Shared secret; clients send `Authorization: Bearer <token>`. |
| `GODOT_LAUNCHER_GODOT_EXE` | yes | Absolute path to Windows Godot under drvfs, e.g. `/mnt/c/Apps/Godot/Godot_v4.6-stable_win64.exe`. |
| `GODOT_LAUNCHER_ALLOWLIST_PREFIXES` | yes | Comma-separated **existing** directory prefixes (each is `canonicalize`d at startup). Project root must lie under one of them. |
| `GODOT_LAUNCHER_LISTEN` | no | Default `127.0.0.1:27182`. |
| `GODOT_LAUNCHER_WORKSPACE_LINUX` | no\* | Canonical Linux root for the repo in the dev container, e.g. `/workspaces/minimap`. |
| `GODOT_LAUNCHER_WORKSPACE_WINDOWS` | no\* | Matching Windows path for `--path`, e.g. `C:\dev\games\minimap`. |

\*If you use `/workspaces/...` paths from the container, set **both** workspace variables so the server can map them to a Windows `--path`. If the project lives only under `/mnt/c/...`, you can omit them.

See [`.env.example`](.env.example) for a template.

## API

All routes require `Authorization: Bearer <GODOT_LAUNCHER_TOKEN>`.

- `GET /health` → `{ "status": "ok" }`
- `POST /sessions` with body `{ "project_root": "<absolute linux path>" }` → `{ "id": "<uuid>", "pid": <linux pid optional> }`
- `DELETE /sessions/<uuid>` → `204` and terminates the session’s Godot (SIGTERM, then kill).

## Connectivity

With WSL **mirrored networking**, the dev container often reaches the service at the same URL as WSL loopback (e.g. `http://127.0.0.1:27182`). Confirm once from inside the container:

```bash
curl -sS -H "Authorization: Bearer $GODOT_REMOTE_TOKEN" "$GODOT_REMOTE_URL/health"
```

## Security notes

- Bind stays on **127.0.0.1** by default; do not widen without TLS and firewall planning.
- **Malicious Godot projects** can still run code once headless Godot loads them—the launcher only restricts **who can start Godot and from which directories**.
