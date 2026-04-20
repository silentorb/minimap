//! Headless Godot launcher: HTTP on loopback, argv-only Windows Godot spawn from WSL.

mod config;
mod paths;

use std::collections::HashMap;
use std::path::PathBuf;
use std::sync::Arc;
use std::time::Duration;

use axum::{
    extract::{Path, State},
    http::StatusCode,
    response::IntoResponse,
    routing::{delete, get, post},
    Json, Router,
};
use serde::{Deserialize, Serialize};
use tokio::process::Command;
use uuid::Uuid;

use config::Config;

#[derive(Clone)]
struct AppState {
    cfg: Arc<Config>,
    sessions: Arc<tokio::sync::Mutex<HashMap<Uuid, tokio::process::Child>>>,
}

async fn health() -> impl IntoResponse {
    Json(serde_json::json!({"status":"ok"})).into_response()
}

#[derive(Deserialize)]
#[serde(deny_unknown_fields)]
struct CreateSessionBody {
    project_root: String,
}

#[derive(Serialize)]
struct SessionResponse {
    id: Uuid,
    #[serde(skip_serializing_if = "Option::is_none")]
    pid: Option<u32>,
}

async fn create_session(
    State(state): State<Arc<AppState>>,
    Json(body): Json<CreateSessionBody>,
) -> impl IntoResponse {
    let raw = PathBuf::from(body.project_root.trim());
    let project_canonical = match std::fs::canonicalize(&raw) {
        Ok(p) => p,
        Err(e) => {
            return (
                StatusCode::BAD_REQUEST,
                Json(serde_json::json!({"error":"invalid_project_root","detail": e.to_string()})),
            )
                .into_response();
        }
    };

    let windows_path = match paths::linux_to_windows_path(
        &project_canonical,
        state.cfg.workspace_linux.as_deref(),
        state.cfg.workspace_windows.as_deref(),
    ) {
        Ok(p) => p,
        Err(e) => {
            return (
                StatusCode::BAD_REQUEST,
                Json(serde_json::json!({"error":"windows_path_mapping","detail": e.to_string()})),
            )
                .into_response();
        }
    };

    let mut cmd = Command::new(state.cfg.godot_exe.as_os_str());
    cmd.arg("--headless");
    cmd.arg("--path");
    cmd.arg(&windows_path);

    let child = match cmd.spawn() {
        Ok(c) => c,
        Err(e) => {
            tracing::error!(error = %e, "spawn Godot failed");
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(serde_json::json!({"error":"spawn_failed","detail": e.to_string()})),
            )
                .into_response();
        }
    };

    let pid = child.id();
    let id = Uuid::new_v4();
    state.sessions.lock().await.insert(id, child);

    (
        StatusCode::CREATED,
        Json(SessionResponse { id, pid }),
    )
        .into_response()
}

async fn delete_session(
    State(state): State<Arc<AppState>>,
    Path(id): Path<Uuid>,
) -> impl IntoResponse {
    let mut child = match state.sessions.lock().await.remove(&id) {
        Some(c) => c,
        None => return StatusCode::NOT_FOUND.into_response(),
    };

    terminate_child(&mut child).await;
    StatusCode::NO_CONTENT.into_response()
}

async fn terminate_child(child: &mut tokio::process::Child) {
    #[cfg(unix)]
    if let Some(pid) = child.id() {
        let _ = nix::sys::signal::kill(
            nix::unistd::Pid::from_raw(pid as i32),
            nix::sys::signal::Signal::SIGTERM,
        );
    }

    match tokio::time::timeout(Duration::from_secs(10), child.wait()).await {
        Ok(Ok(_)) => return,
        Ok(Err(e)) => tracing::warn!(error = %e, "wait after SIGTERM"),
        Err(_) => tracing::info!("SIGTERM wait timed out; sending kill"),
    }

    if let Err(e) = child.kill().await {
        tracing::warn!(error = %e, "kill failed (process may have exited)");
    }
    let _ = child.wait().await;
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    tracing_subscriber::fmt()
        .with_env_filter(
            tracing_subscriber::EnvFilter::try_from_default_env()
                .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info")),
        )
        .init();

    let cfg = Arc::new(Config::from_env()?);
    tracing::info!(listen = %cfg.listen, godot_exe = ?cfg.godot_exe, "godot-launcher starting");

    let state = Arc::new(AppState {
        cfg: cfg.clone(),
        sessions: Arc::new(tokio::sync::Mutex::new(HashMap::new())),
    });

    let app = Router::new()
        .route("/health", get(health))
        .route("/sessions", post(create_session))
        .route("/sessions/:id", delete(delete_session))
        .with_state(state);

    let listener = tokio::net::TcpListener::bind(&cfg.listen).await?;
    tracing::info!("listening on {}", cfg.listen);
    axum::serve(listener, app).await?;
    Ok(())
}
