//! Map validated Linux paths to a single Windows `--path` string for Godot.exe.

use std::path::Path;

use thiserror::Error;

#[derive(Debug, Error)]
pub enum PathMapError {
    #[error("path is not under /mnt/<drive>/... and no workspace Windows mapping applies")]
    NoWindowsMapping,
}

/// Convert a canonical Linux project directory to the Windows form Godot expects in `--path`.
///
/// - `/mnt/c/Users/foo/bar` → `C:\Users\foo\bar`
/// - If `workspace_linux` + `workspace_windows` are set and `linux` is under `workspace_linux`,
///   replace that prefix with `workspace_windows` and use backslashes.
pub fn linux_to_windows_path(
    linux: &Path,
    workspace_linux: Option<&Path>,
    workspace_windows: Option<&str>,
) -> Result<String, PathMapError> {
    let s = linux.to_string_lossy();
    if let Some(win) = mnt_to_windows(&s) {
        return Ok(win);
    }
    if let (Some(wl), Some(ww)) = (workspace_linux, workspace_windows) {
        let wl_s = wl.to_string_lossy();
        if s.starts_with(wl_s.as_ref()) {
            let tail = &s[wl_s.len()..];
            let tail = tail.trim_start_matches('/').replace('/', "\\");
            let base = ww.trim_end_matches(['\\', '/']);
            let out = if tail.is_empty() {
                base.to_string()
            } else {
                format!("{base}\\{tail}")
            };
            return Ok(out);
        }
    }
    Err(PathMapError::NoWindowsMapping)
}

/// `/mnt/<drive>/rest` → `<DRIVE>:\rest` with backslashes.
fn mnt_to_windows(s: &str) -> Option<String> {
    let lower = s.to_ascii_lowercase();
    let prefix = "/mnt/";
    if !lower.starts_with(prefix) {
        return None;
    }
    let bytes = s.as_bytes();
    if bytes.len() < 7 || bytes.get(6) != Some(&b'/') {
        return None;
    }
    let drive = bytes[5] as char;
    if !drive.is_ascii_alphabetic() {
        return None;
    }
    let rest = &s[7..];
    let rest = rest.replace('/', "\\");
    Some(format!("{}:\\{}", drive.to_ascii_uppercase(), rest))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn mnt_c_users() {
        let win = mnt_to_windows("/mnt/c/Users/x/repo").unwrap();
        assert_eq!(win, "C:\\Users\\x\\repo");
    }

    #[test]
    fn mnt_uppercase_drive() {
        let win = mnt_to_windows("/mnt/E/games/marloth").unwrap();
        assert_eq!(win, "E:\\games\\marloth");
    }
}
