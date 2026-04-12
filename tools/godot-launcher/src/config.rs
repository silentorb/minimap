//! Environment-driven configuration (no arbitrary client paths into argv).

use std::path::PathBuf;

use thiserror::Error;

#[derive(Debug, Clone)]
pub struct Config {
    pub token: String,
    pub godot_exe: PathBuf,
    /// Canonical path prefixes; `project_root` must be under one of these after canonicalize.
    pub allowlist_prefixes: Vec<PathBuf>,
    pub listen: String,
    /// If set with `workspace_windows`, map `/workspaces/...` to a Windows root for `--path`.
    pub workspace_linux: Option<PathBuf>,
    pub workspace_windows: Option<String>,
}

#[derive(Debug, Error)]
pub enum ConfigError {
    #[error("missing or empty environment variable: {0}")]
    MissingEnv(&'static str),
    #[error("allowlist prefix does not exist or could not be canonicalized: {0}")]
    BadAllowlistPrefix(String),
    #[error("GODOT_LAUNCHER_WORKSPACE_LINUX and GODOT_LAUNCHER_WORKSPACE_WINDOWS must both be set or both unset")]
    WorkspacePair,
}

impl Config {
    pub fn from_env() -> Result<Self, ConfigError> {
        let token = std::env::var("GODOT_LAUNCHER_TOKEN")
            .map_err(|_| ConfigError::MissingEnv("GODOT_LAUNCHER_TOKEN"))?;
        if token.is_empty() {
            return Err(ConfigError::MissingEnv("GODOT_LAUNCHER_TOKEN"));
        }

        let godot_exe = PathBuf::from(
            std::env::var("GODOT_LAUNCHER_GODOT_EXE")
                .map_err(|_| ConfigError::MissingEnv("GODOT_LAUNCHER_GODOT_EXE"))?,
        );
        if godot_exe.as_os_str().is_empty() {
            return Err(ConfigError::MissingEnv("GODOT_LAUNCHER_GODOT_EXE"));
        }

        let raw_allow = std::env::var("GODOT_LAUNCHER_ALLOWLIST_PREFIXES")
            .map_err(|_| ConfigError::MissingEnv("GODOT_LAUNCHER_ALLOWLIST_PREFIXES"))?;
        let mut allowlist_prefixes = Vec::new();
        for part in raw_allow.split(',') {
            let p = part.trim();
            if p.is_empty() {
                continue;
            }
            let pb = PathBuf::from(p);
            let canon = std::fs::canonicalize(&pb)
                .map_err(|_| ConfigError::BadAllowlistPrefix(p.to_string()))?;
            allowlist_prefixes.push(canon);
        }
        if allowlist_prefixes.is_empty() {
            return Err(ConfigError::MissingEnv("GODOT_LAUNCHER_ALLOWLIST_PREFIXES"));
        }

        let listen = std::env::var("GODOT_LAUNCHER_LISTEN")
            .unwrap_or_else(|_| "127.0.0.1:27182".to_string());

        let wl = std::env::var("GODOT_LAUNCHER_WORKSPACE_LINUX").ok();
        let ww = std::env::var("GODOT_LAUNCHER_WORKSPACE_WINDOWS").ok();
        let (workspace_linux, workspace_windows) = match (wl, ww) {
            (None, None) => (None, None),
            (Some(l), Some(w)) if !l.is_empty() && !w.is_empty() => {
                let lp = PathBuf::from(&l);
                let canon = std::fs::canonicalize(&lp)
                    .map_err(|_| ConfigError::BadAllowlistPrefix(l))?;
                (Some(canon), Some(w))
            }
            _ => return Err(ConfigError::WorkspacePair),
        };

        Ok(Config {
            token,
            godot_exe,
            allowlist_prefixes,
            listen,
            workspace_linux,
            workspace_windows,
        })
    }

    pub fn project_allowed(&self, project_canonical: &std::path::Path) -> bool {
        self.allowlist_prefixes
            .iter()
            .any(|prefix| project_canonical.starts_with(prefix))
    }
}
