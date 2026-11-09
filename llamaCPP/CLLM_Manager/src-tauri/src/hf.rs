use futures_util::StreamExt;
use serde::Serialize;
use std::io::Write;
use std::sync::atomic::{AtomicBool, Ordering};
use tauri::{AppHandle, Emitter};

#[derive(Serialize, Clone)]
pub struct HfFile {
    pub path: String,
    pub size: u64,
}

static CANCEL: AtomicBool = AtomicBool::new(false);

pub fn cancel() {
    CANCEL.store(true, Ordering::SeqCst);
}

fn auth(token: &Option<String>) -> Option<String> {
    token.as_ref().map(|t| format!("Bearer {t}"))
}

fn emit_progress(
    app: &AppHandle,
    received: u64,
    total: u64,
    speed: f64,
    done: bool,
    error: Option<String>,
) {
    let _ = app.emit(
        "hf-progress",
        serde_json::json!({
            "received": received,
            "total": total,
            "speed": speed,
            "done": done,
            "error": error,
        }),
    );
}

/// Список .gguf файлов репозитория.
pub async fn list_files(token: &Option<String>, repo: &str) -> Result<Vec<HfFile>, String> {
    let repo = repo.trim();
    if repo.is_empty() {
        return Err("Укажите репозиторий (например, Qwen/Qwen3-32B-GGUF)".into());
    }
    let url = format!("https://huggingface.co/api/models/{repo}/tree/main?recursive=true");
    let client = reqwest::Client::new();
    let mut req = client.get(&url);
    if let Some(h) = auth(token) {
        req = req.header("Authorization", h);
    }
    let resp = req.send().await.map_err(|e| format!("Сеть: {e}"))?;
    let status = resp.status();
    if !status.is_success() {
        return Err(format!(
            "HF API: HTTP {status} — проверьте имя репозитория и (для gated) токен"
        ));
    }
    let items: Vec<serde_json::Value> = resp.json().await.map_err(|e| e.to_string())?;
    let mut out = Vec::new();
    for it in items {
        if it.get("type").and_then(|t| t.as_str()) != Some("file") {
            continue;
        }
        if let (Some(path), Some(size)) = (
            it.get("path").and_then(|p| p.as_str()),
            it.get("size").and_then(|s| s.as_u64()),
        ) {
            if path.to_lowercase().ends_with(".gguf") {
                out.push(HfFile {
                    path: path.to_string(),
                    size,
                });
            }
        }
    }
    out.sort_by(|a, b| a.path.cmp(&b.path));
    Ok(out)
}

/// Скачивание файла с resume (продолжением) и стримингом прогресса.
pub async fn download(
    app: &AppHandle,
    token: &Option<String>,
    repo: &str,
    file: &str,
    dest_dir: &str,
) -> Result<(), String> {
    CANCEL.store(false, Ordering::SeqCst);
    let url = format!("https://huggingface.co/{repo}/resolve/main/{file}");
    std::fs::create_dir_all(dest_dir).map_err(|e| e.to_string())?;

    // имя файла — последний сегмент пути
    let name = file
        .rsplit('/')
        .next()
        .filter(|s| !s.is_empty())
        .unwrap_or("model.gguf");
    let dest = std::path::Path::new(dest_dir).join(name);

    let resume_from = if dest.exists() {
        dest.metadata().map(|m| m.len()).unwrap_or(0)
    } else {
        0
    };

    let client = reqwest::Client::new();
    let mut req = client
        .get(&url)
        .header("Range", format!("bytes={resume_from}-"));
    if let Some(h) = auth(token) {
        req = req.header("Authorization", h);
    }
    let resp = req.send().await.map_err(|e| format!("Сеть: {e}"))?;
    let status = resp.status();
    if status.as_u16() == 200 && resume_from > 0 {
        // Range не поддержан — начинаем заново
        std::fs::File::create(&dest).map_err(|e| e.to_string())?;
    }
    if !status.is_success() && status.as_u16() != 206 {
        return Err(format!(
            "HF: HTTP {status} — проверьте имя репозитория/файла и токен"
        ));
    }
    let total: u64 = resp
        .headers()
        .get("content-range")
        .and_then(|v| v.to_str().ok())
        .and_then(|s| s.rsplit('/').next())
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);

    let mut out_file = std::fs::OpenOptions::new()
        .create(true)
        .append(true)
        .open(&dest)
        .map_err(|e| e.to_string())?;

    let mut stream = resp.bytes_stream();
    let mut received = resume_from;
    let bytes_start = received;
    let t0 = std::time::Instant::now();
    let mut last_emit = t0;

    while let Some(chunk) = stream.next().await {
        if CANCEL.load(Ordering::SeqCst) {
            emit_progress(app, received, total, 0.0, true, Some("Отменено пользователем".into()));
            return Ok(());
        }
        let chunk = chunk.map_err(|e| format!("Сеть: {e}"))?;
        out_file
            .write_all(&chunk)
            .map_err(|e| format!("Запись файла: {e}"))?;
        received += chunk.len() as u64;
        if last_emit.elapsed() > std::time::Duration::from_millis(400) {
            let speed = (received - bytes_start) as f64 / t0.elapsed().as_secs_f64();
            emit_progress(app, received, total, speed, false, None);
            last_emit = std::time::Instant::now();
        }
    }

    emit_progress(app, received, total, 0.0, true, None);
    Ok(())
}
