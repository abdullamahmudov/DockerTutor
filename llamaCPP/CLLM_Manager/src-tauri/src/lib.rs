mod config;
mod docker;
mod hf;
mod models;
mod params;

use config::{AppConfig, Profile};

pub fn run() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![
            get_config,
            save_config,
            get_param_defs,
            get_profiles,
            save_profiles,
            new_profile,
            list_models,
            docker_available,
            list_containers,
            start_container,
            stop_container,
            open_logs,
            build_image,
            hf_list_files,
            hf_download,
            hf_cancel,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

// ---------- Конфигурация ----------

#[tauri::command]
fn get_config() -> Result<AppConfig, String> {
    config::load_config()
}

#[tauri::command]
fn save_config(cfg: AppConfig) -> Result<(), String> {
    config::save_config(&cfg)
}

#[tauri::command]
fn get_param_defs() -> Vec<params::ParamDef> {
    params::catalog()
}

// ---------- Профили ----------

#[tauri::command]
fn get_profiles() -> Result<Vec<Profile>, String> {
    config::load_profiles()
}

#[tauri::command]
fn save_profiles(profiles: Vec<Profile>) -> Result<(), String> {
    config::save_profiles(&profiles)
}

#[tauri::command]
fn new_profile(port_base: u16, count: usize) -> Profile {
    config::new_profile(port_base, count)
}

// ---------- Модели ----------

#[tauri::command]
fn list_models() -> Result<Vec<models::ModelInfo>, String> {
    let cfg = config::load_config()?;
    models::list_models(&cfg.models_path)
}

// ---------- Docker ----------

#[tauri::command]
async fn docker_available() -> bool {
    docker::docker_available().await
}

#[tauri::command]
async fn list_containers() -> Result<Vec<docker::ContainerStatus>, String> {
    let profiles = config::load_profiles()?;
    docker::list(&profiles).await
}

#[tauri::command]
async fn start_container(app: tauri::AppHandle, name: String) -> Result<(), String> {
    let cfg = config::load_config()?;
    let profiles = config::load_profiles()?;
    let profile = profiles
        .iter()
        .find(|p| p.name == name)
        .ok_or_else(|| format!("Профиль не найден: {name}"))?
        .clone();
    docker::start(&app, &cfg, &profile).await
}

#[tauri::command]
async fn stop_container(name: String) -> Result<(), String> {
    docker::stop(&name).await
}

#[tauri::command]
async fn open_logs(app: tauri::AppHandle, name: String) -> Result<(), String> {
    docker::open_logs(&app, &name).await
}

#[tauri::command]
async fn build_image(app: tauri::AppHandle, cfg: AppConfig) -> Result<(), String> {
    // сохраняем переданную конфигурацию и собираем по ней
    config::save_config(&cfg)?;
    docker::build_image(&app, &cfg).await
}

// ---------- HuggingFace ----------

#[tauri::command]
async fn hf_list_files(repo: String) -> Result<Vec<hf::HfFile>, String> {
    let cfg = config::load_config()?;
    hf::list_files(&cfg.hf_token, &repo).await
}

#[tauri::command]
async fn hf_download(app: tauri::AppHandle, repo: String, file: String) -> Result<(), String> {
    let cfg = config::load_config()?;
    hf::download(&app, &cfg.hf_token, &repo, &file, &cfg.models_path).await
}

#[tauri::command]
fn hf_cancel() {
    hf::cancel();
}
