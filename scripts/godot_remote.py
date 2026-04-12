#!/usr/bin/env python3
"""Thin stdlib client for tools/godot-launcher (see tools/godot-launcher/README.md)."""

from __future__ import annotations

import argparse
import json
import os
import sys
import urllib.error
import urllib.request


def _require_env(name: str) -> str:
    v = os.environ.get(name)
    if not v:
        print(f"{name} is not set", file=sys.stderr)
        sys.exit(2)
    return v


def _request(
    method: str,
    path: str,
    *,
    body: dict | None = None,
) -> tuple[int, str]:
    base = _require_env("GODOT_REMOTE_URL").rstrip("/")
    token = _require_env("GODOT_REMOTE_TOKEN")
    url = f"{base}{path}"
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={
            "Authorization": f"Bearer {token}",
            **({"Content-Type": "application/json"} if body is not None else {}),
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            raw = resp.read().decode("utf-8")
            return resp.status, raw
    except urllib.error.HTTPError as e:
        raw = e.read().decode("utf-8", errors="replace")
        return e.code, raw


def cmd_health() -> int:
    code, text = _request("GET", "/health")
    print(text)
    return 0 if code == 200 else 1


def cmd_start(project_root: str) -> int:
    code, text = _request("POST", "/sessions", body={"project_root": project_root})
    print(text)
    if code != 201:
        return 1
    try:
        sid = json.loads(text)["id"]
    except (json.JSONDecodeError, KeyError):
        print("response missing id", file=sys.stderr)
        return 1
    print(f"session_id={sid}", file=sys.stderr)
    return 0


def cmd_stop(session_id: str) -> int:
    code, text = _request("DELETE", f"/sessions/{session_id}")
    if text:
        print(text)
    return 0 if code == 204 else 1


def main() -> int:
    p = argparse.ArgumentParser(description="Call godot-launcher from the dev container.")
    sub = p.add_subparsers(dest="cmd", required=True)

    sub.add_parser("health", help="GET /health")

    sp = sub.add_parser("start", help="POST /sessions (headless Godot)")
    sp.add_argument(
        "project_root",
        nargs="?",
        default=os.getcwd(),
        help="Absolute Linux path to the Godot project (default: cwd)",
    )

    st = sub.add_parser("stop", help="DELETE /sessions/<id>")
    st.add_argument("session_id", help="UUID printed by start")

    args = p.parse_args()
    if args.cmd == "health":
        return cmd_health()
    if args.cmd == "start":
        return cmd_start(os.path.abspath(args.project_root))
    if args.cmd == "stop":
        return cmd_stop(args.session_id)
    return 2


if __name__ == "__main__":
    raise SystemExit(main())
