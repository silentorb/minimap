//! Environment-driven configuration (argv-only Windows Godot spawn).

use std::path::PathBuf;

use thiserror::Error;

#[derive(Debug, Clone)]
pub struct Config {
    pub godot_exe: PathBuf,
    pub listen: String,
    /// If set with `workspace_windows`, map `/workspaces/...` to a Windows root for `--path`.
    pub workspace_linux: Option<PathBuf>,
    pub workspace_windows: Option<String>,
}

#[derive(Debug, Error)]
pub enum ConfigError {
    #[error("missing or empty environment variable: {0}")]
    MissingEnv(&'static str),
    #[error("path does not exist or could not be canonicalized: {0}")]
    BadPath(String),
    #[error("GODOT_LAUNCHER_WORKSPACE_LINUX and GODOT_LAUNCHER_WORKSPACE_WINDOWS must both be set or both unset")]
    WorkspacePair,
}

impl Config {
    pub fn from_env() -> Result<Self, ConfigError> {
        let godot_exe = PathBuf::from(
            std::env::var("GODOT_LAUNCHER_GODOT_EXE")
                .map_err(|_| ConfigError::MissingEnv("GODOT_LAUNCHER_GODOT_EXE"))?,
        );
        if godot_exe.as_os_str().is_empty() {
            return Err(ConfigError::MissingEnv("GODOT_LAUNCHER_GODOT_EXE"));
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
                    .map_err(|_| ConfigError::BadPath(l))?;
                (Some(canon), Some(w))
            }
            _ => return Err(ConfigError::WorkspacePair),
        };

        Ok(Config {
            godot_exe,
            listen,
            workspace_linux,
            workspace_windows,
        })
    }
}
