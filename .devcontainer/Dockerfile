FROM mcr.microsoft.com/devcontainers/base:debian AS base

# By default /workspaces is owned by `root`.
# When the Dev Container extension mounts the source directory, it changes the mounted file owner to `vscode`.
# A directory is created under /workspaces which is owned by `vscode` and acts as a scratch pad to store
# miscellaneous ephemeral files.
RUN <<EOF
mkdir -p /workspaces/dev /workspaces/build
chown vscode:vscode /workspaces/dev /workspaces/build
EOF

# python3 for general scripting; curl + ca-certificates for HTTPS downloads;
# unzip + X11/GL/font libs for the Godot .NET Linux editor binary (headless tests).
RUN <<EOF
apt-get update
apt-get upgrade -y
apt-get install -y --no-install-recommends \
    python3 \
    curl \
    ca-certificates \
    unzip \
    libx11-6 \
    libxcursor1 \
    libxi6 \
    libxinerama1 \
    libxrandr2 \
    libgl1 \
    libfontconfig1
rm -rf /var/lib/apt/lists/*
EOF

# Godot 4.6 .NET (mono) Linux x86_64 — binary + GodotSharp/ data directory.
ARG GODOT_VERSION=4.6-stable
ARG GODOT_ZIP=Godot_v4.6-stable_mono_linux_x86_64.zip
RUN <<EOF
set -euo pipefail
tmpdir="$(mktemp -d)"
curl -fsSL -o "${tmpdir}/godot.zip" \
  "https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}/${GODOT_ZIP}"
unzip -q "${tmpdir}/godot.zip" -d "${tmpdir}"
mkdir -p /opt/godot
mv "${tmpdir}/Godot_v4.6-stable_mono_linux_x86_64"/* /opt/godot/
chmod +x /opt/godot/Godot_v4.6-stable_mono_linux.x86_64
rm -rf "${tmpdir}"
EOF
