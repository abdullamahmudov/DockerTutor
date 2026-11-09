use serde::Serialize;
use std::collections::HashMap;
use std::process::Stdio;
use std::sync::Mutex;
use tauri::{AppHandle, Emitter};

use crate::config::{AppConfig, Profile};

#[derive(Serialize, Clone)]
pub struct ContainerStatus {
    pub profile: String,
    pub running: bool,
    pub state: String,
    pub port: u16,
    pub model: String,
}

/// Имя контейнера для профиля.
pub fn container_name(profile: &str) -> String {
    let sanitized: String = profile
        .chars()
        .map(|c| if c.is_ascii_alphanumeric() || c == '-' || c == '_' { c } else { '-' })
        .collect();
    format!("cllm-mgr-{sanitized}")
}

/// Активные процессы `docker logs -f` (по одному на просматриваемый профиль).
static LOG_TAILERS: std::sync::LazyLock<Mutex<HashMap<String, tokio::process::Child>>> =
    std::sync::LazyLock::new(|| Mutex::new(HashMap::new()));

async fn run_docker(args: &[String]) -> Result<String, String> {
    let out = tokio::process::Command::new("docker")
        .args(args)
        .output()
        .await
        .map_err(|e| format!("Не удалось запустить docker: {e}"))?;
    if !out.status.success() {
        let err = String::from_utf8_lossy(&out.stderr).trim().to_string();
        return Err(format!(
            "docker {} → ошибка:\n{err}",
            args.iter().map(|a| shell_quote(a)).collect::<Vec<_>>().join(" ")
        ));
    }
    Ok(String::from_utf8_lossy(&out.stdout).trim().to_string())
}

fn shell_quote(s: &str) -> String {
    if s.chars().any(|c| c.is_whitespace() || c == '"' || c == '\'') {
        format!("{s:?}")
    } else {
        s.to_string()
    }
}

pub async fn docker_available() -> bool {
    run_docker(&["version".into(), "--format".into(), "{{.Server.Version}}".into()])
        .await
        .is_ok()
}

/// Статусы контейнеров всех профилей.
pub async fn list(profiles: &[Profile]) -> Result<Vec<ContainerStatus>, String> {
    let out = run_docker(
        &[
            "ps".into(),
            "-a".into(),
            "--filter".into(),
            "name=cllm-mgr-".into(),
            "--format".into(),
            "{{.Names}}\t{{.State}}".into(),
        ],
    )
    .await;
    let map: HashMap<String, String> = match out {
        Ok(o) => o
            .lines()
            .filter_map(|l| {
                let mut it = l.splitn(2, '\t');
                Some((it.next()?.to_string(), it.next()?.to_string()))
            })
            .collect(),
        Err(_) => HashMap::new(),
    };
    Ok(profiles
        .iter()
        .map(|p| {
            let cn = container_name(&p.name);
            let state = map.get(&cn).cloned().unwrap_or_else(|| "нет контейнера".into());
            ContainerStatus {
                profile: p.name.clone(),
                running: state == "running",
                state,
                port: p.port,
                model: p.model.clone(),
            }
        })
        .collect())
}

/// Запуск контейнера профиля.
pub async fn start(app: &AppHandle, cfg: &AppConfig, profile: &Profile) -> Result<(), String> {
    if profile.model.is_empty() {
        return Err("В профиле не выбрана модель".into());
    }
    let cn = container_name(&profile.name);

    // 1. Модель должна существовать
    let model_file = crate::models::model_path(&cfg.models_path, &profile.model)?;

    // 2. Порт должен быть свободен
    if std::net::TcpListener::bind(("127.0.0.1", profile.port)).is_err() {
        return Err(format!(
            "Порт {} уже занят другим процессом",
            profile.port
        ));
    }

    // 3. Убираем старый контейнер с таким именем (если есть)
    let _ = run_docker(&["rm".into(), "-f".into(), cn.clone()]).await;

    // 4. Собираем аргументы llama-server
    let mut server_args: Vec<String> = vec![
        "-m".into(),
        format!("/models/{}", model_file.file_name().unwrap().to_string_lossy()),
        "--host".into(),
        "0.0.0.0".into(),
        "--port".into(),
        "8080".into(),
    ];
    for p in &profile.params {
        if !p.enabled {
            continue; // выключенные параметры не попадают в команду
        }
        server_args.push(p.flag.clone());
        if let Some(v) = &p.value {
            if !v.is_empty() {
                server_args.push(v.clone());
            }
        }
    }
    server_args.extend(profile.raw_args.iter().cloned());

    // 5. docker run
    let image = profile
        .image
        .clone()
        .filter(|s| !s.is_empty())
        .unwrap_or_else(|| cfg.image.clone());
    let models_vol = format!(
        "{}:/models:ro",
        cfg.models_path.replace('\\', "/")
    );
    let mut args: Vec<String> = vec![
        "run".into(),
        "-d".into(),
        "--name".into(),
        cn.clone(),
        "--gpus".into(),
        "all".into(),
        "-p".into(),
        format!("{}:8080", profile.port),
        "-v".into(),
        models_vol,
        image,
    ];
    args.extend(server_args);

    let _id = run_docker(&args)
        .await
        .map_err(|e| format!("Ошибка запуска контейнера:\n{e}"))?;

    // 6. Запускаем live-логи
    spawn_log_tailer(app, &profile.name, &cn).await;
    Ok(())
}

/// Остановка контейнера профиля.
pub async fn stop(profile_name: &str) -> Result<(), String> {
    let cn = container_name(profile_name);
    kill_log_tailer(profile_name);
    run_docker(&["stop".into(), "-t".into(), "30".into(), cn])
        .await
        .map(|_| ())
}

/// Переименование профиля: переименовывает контейнер (если существует, работает
/// и для запущенного) и перезапускает log-tailer под новым именем.
pub async fn rename(app: &AppHandle, from: &str, to: &str) -> Result<(), String> {
    let cn_from = container_name(from);
    let cn_to = container_name(to);
    let exists = run_docker(&["inspect".into(), cn_from.clone()]).await.is_ok();
    if exists {
        run_docker(&["rename".into(), cn_from, cn_to.clone()])
            .await
            .map_err(|e| format!("Не удалось переименовать контейнер:\n{e}"))?;
    }
    kill_log_tailer(from);
    if exists {
        open_logs(app, to).await?;
    }
    Ok(())
}

/// Открыть live-логи профиля (заменяет предыдущий tailer).
pub async fn open_logs(app: &AppHandle, profile_name: &str) -> Result<(), String> {
    let cn = container_name(profile_name);
    let exists = run_docker(&["inspect".into(), cn.clone()]).await.is_ok();
    kill_log_tailer(profile_name);
    if !exists {
        return Err("Контейнер не существует — сначала запустите профиль".into());
    }
    spawn_log_tailer(app, profile_name, &cn).await;
    Ok(())
}

/// Запуск процесса `docker logs -f` и стриминг строк в UI (событие "container-log").
async fn spawn_log_tailer(app: &AppHandle, profile: &str, cn: &str) {
    let app = app.clone();
    let profile = profile.to_string();
    let cn = cn.to_string();
    tokio::spawn(async move {
        let child = match tokio::process::Command::new("docker")
            .args(["logs", "-f", "--tail", "300", &cn])
            .stdout(Stdio::piped())
            .stderr(Stdio::null())
            .spawn()
        {
            Ok(c) => c,
            Err(_) => return,
        };

        // Процесс регистрируем (чтобы kill из stop/open_logs), stdout забираем из реестра
        let stdout = {
            let mut map = LOG_TAILERS.lock().unwrap();
            map.insert(profile.clone(), child);
            map.get_mut(&profile).and_then(|c| c.stdout.take())
        };

        if let Some(mut stdout) = stdout {
            use tokio::io::AsyncReadExt;
            let mut buf = vec![0u8; 8192];
            loop {
                match stdout.read(&mut buf).await {
                    Ok(0) => break,
                    Ok(n) => {
                        let text = String::from_utf8_lossy(&buf[..n]);
                        for line in text.lines() {
                            let _ = app.emit(
                                "container-log",
                                serde_json::json!({
                                    "profile": profile,
                                    "line": line,
                                }),
                            );
                        }
                    }
                    Err(_) => break,
                }
            }
        }

        // Убираем процесс из реестра (если его ещё не убили снаружи)
        let child_opt = {
            let mut map = LOG_TAILERS.lock().unwrap();
            map.remove(&profile)
        };
        if let Some(mut child) = child_opt {
            let _ = child.wait().await;
        }
    });
}

fn kill_log_tailer(profile: &str) {
    let mut map = LOG_TAILERS.lock().unwrap();
    if let Some(mut child) = map.remove(profile) {
        let _ = child.start_kill();
    }
}

/// Сборка Docker-образа из Dockerfile (стрим лога в событие "build-log").
pub async fn build_image(app: &AppHandle, cfg: &AppConfig) -> Result<(), String> {
    let dockerfile = std::path::Path::new(&cfg.dockerfile);
    if !dockerfile.is_file() {
        return Err(format!("Dockerfile не найден: {}", cfg.dockerfile));
    }
    let context = dockerfile
        .parent()
        .map(|p| p.to_string_lossy().to_string())
        .unwrap_or_else(|| ".".into());
    let dockerfile_s = cfg.dockerfile.clone();
    let image = cfg.image.clone();
    let cuda_version = cfg.build_args.cuda_version.clone();
    let ubuntu_version = cfg.build_args.ubuntu_version.clone();
    let llama_cpp_ref = cfg.build_args.llama_cpp_ref.clone();
    let cuda_arch = cfg.build_args.cuda_arch.clone();
    let app = app.clone();

    tokio::spawn(async move {
        let emit_line = |line: String| {
            let _ = app.emit("build-log", serde_json::json!({ "line": line }));
        };
        let mut child = match tokio::process::Command::new("docker")
            .args([
                "build",
                "--pull",
                "-f",
                &dockerfile_s,
                "-t",
                &image,
                "--build-arg",
                &format!("CUDA_VERSION={cuda_version}"),
                "--build-arg",
                &format!("UBUNTU_VERSION={ubuntu_version}"),
                "--build-arg",
                &format!("LLAMA_CPP_REF={llama_cpp_ref}"),
                "--build-arg",
                &format!("CUDA_ARCH={cuda_arch}"),
                &context,
            ])
            .stdout(Stdio::piped())
            .stderr(Stdio::piped())
            .spawn()
        {
            Ok(c) => c,
            Err(e) => {
                emit_line(format!("Не удалось запустить docker build: {e}"));
                return;
            }
        };

        // читаем stdout и stderr параллельно
        let stdout_opt = child.stdout.take();
        let stderr_opt = child.stderr.take();
        let app_out = app.clone();
        let t1 = tokio::spawn(async move {
            if let Some(mut s) = stdout_opt {
                use tokio::io::AsyncReadExt;
                let mut buf = [0u8; 4096];
                loop {
                    match s.read(&mut buf).await {
                        Ok(0) => break,
                        Ok(n) => {
                            for line in String::from_utf8_lossy(&buf[..n]).lines() {
                                let _ = app_out.emit(
                                    "build-log",
                                    serde_json::json!({ "line": line.to_string() }),
                                );
                            }
                        }
                        Err(_) => break,
                    }
                }
            }
        });
        let app_err = app.clone();
        let t2 = tokio::spawn(async move {
            if let Some(mut s) = stderr_opt {
                use tokio::io::AsyncReadExt;
                let mut buf = [0u8; 4096];
                loop {
                    match s.read(&mut buf).await {
                        Ok(0) => break,
                        Ok(n) => {
                            for line in String::from_utf8_lossy(&buf[..n]).lines() {
                                let _ = app_err.emit(
                                    "build-log",
                                    serde_json::json!({ "line": line.to_string() }),
                                );
                            }
                        }
                        Err(_) => break,
                    }
                }
            }
        });

        let status = child.wait().await.unwrap_or_default();
        let _ = t1.await;
        let _ = t2.await;
        emit_line(if status.success() {
            format!("✅ Сборка завершена: {image}")
        } else {
            format!("❌ Сборка завершилась с ошибкой (код {status})")
        });
    });

    Ok(())
}


