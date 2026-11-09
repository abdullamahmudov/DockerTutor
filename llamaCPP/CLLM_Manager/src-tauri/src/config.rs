use serde::{Deserialize, Serialize};
use std::path::PathBuf;

/// Конфигурация приложения (config.yaml)
#[derive(Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct AppConfig {
    pub models_path: String,
    pub dockerfile: String,
    pub image: String,
    pub port_base: u16,
    #[serde(default)]
    pub hf_token: Option<String>,
    #[serde(default)]
    pub build_args: BuildArgs,
}

#[derive(Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct BuildArgs {
    pub cuda_version: String,
    pub ubuntu_version: String,
    pub llama_cpp_ref: String,
    pub cuda_arch: String,
}

impl Default for BuildArgs {
    fn default() -> Self {
        Self {
            cuda_version: "11.4.3".into(),
            ubuntu_version: "20.04".into(),
            llama_cpp_ref: "b5900".into(),
            cuda_arch: "70".into(),
        }
    }
}

/// Один параметр llama-server в профиле.
/// `enabled = false` → флаг НЕ попадает в команду, но значение сохраняется.
#[derive(Serialize, Deserialize, Clone)]
pub struct ParamEntry {
    pub flag: String,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub value: Option<String>,
    #[serde(default)]
    pub enabled: bool,
}

/// Профиль запуска: модель + порт + параметры.
#[derive(Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Profile {
    pub name: String,
    pub model: String,
    pub port: u16,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub image: Option<String>,
    #[serde(default)]
    pub params: Vec<ParamEntry>,
    #[serde(default, rename = "rawArgs")]
    pub raw_args: Vec<String>,
}

#[derive(Serialize, Deserialize, Clone, Default)]
pub struct Profiles {
    #[serde(default)]
    pub profiles: Vec<Profile>,
}

/// Каталог, где лежат config.yaml и presets.yaml.
pub fn config_dir() -> PathBuf {
    if let Ok(dir) = std::env::var("CLLM_CONFIG_DIR") {
        return PathBuf::from(dir);
    }
    let exe_dir = std::env::current_exe()
        .ok()
        .and_then(|p| p.parent().map(|d| d.to_path_buf()))
        .unwrap_or_else(|| PathBuf::from("."));
    // dev: exe лежит в target/debug — поднимаемся до корня проекта (package.json + src-tauri)
    if let Some(root) = find_project_root(&exe_dir) {
        return root;
    }
    exe_dir
}

fn find_project_root(start: &std::path::Path) -> Option<PathBuf> {
    let mut cur = Some(start);
    while let Some(d) = cur {
        if d.join("package.json").is_file() && d.join("src-tauri").is_dir() {
            return Some(d.to_path_buf());
        }
        cur = d.parent();
    }
    None
}

fn config_path() -> PathBuf {
    config_dir().join("config.yaml")
}

fn presets_path() -> PathBuf {
    config_dir().join("presets.yaml")
}

pub fn default_config() -> AppConfig {
    let models = r"D:\Work\Custom\DockerTutor\DockerTutor\llamaCPP\models".to_string();
    let dockerfile =
        r"D:\Work\Custom\DockerTutor\DockerTutor\llamaCPP\V100.Dockerfile".to_string();
    AppConfig {
        models_path: models,
        dockerfile,
        image: "llama-cpp-v100:cuda11.4.3".into(),
        port_base: 9722,
        hf_token: None,
        build_args: BuildArgs::default(),
    }
}

pub fn load_config() -> Result<AppConfig, String> {
    let path = config_path();
    if path.exists() {
        let text = std::fs::read_to_string(&path).map_err(|e| e.to_string())?;
        serde_yaml::from_str(&text).map_err(|e| format!("config.yaml: {e}"))
    } else {
        let cfg = default_config();
        save_config(&cfg)?;
        Ok(cfg)
    }
}

pub fn save_config(cfg: &AppConfig) -> Result<(), String> {
    std::fs::create_dir_all(config_dir()).map_err(|e| e.to_string())?;
    let text = serde_yaml::to_string(cfg).map_err(|e| e.to_string())?;
    std::fs::write(config_path(), text).map_err(|e| e.to_string())
}

pub fn load_profiles() -> Result<Vec<Profile>, String> {
    let path = presets_path();
    if !path.exists() {
        return Ok(Vec::new());
    }
    let text = std::fs::read_to_string(&path).map_err(|e| e.to_string())?;
    if text.trim().is_empty() {
        return Ok(Vec::new());
    }
    let p: Profiles = serde_yaml::from_str(&text).map_err(|e| format!("presets.yaml: {e}"))?;
    Ok(p.profiles)
}

pub fn save_profiles(profiles: &[Profile]) -> Result<(), String> {
    std::fs::create_dir_all(config_dir()).map_err(|e| e.to_string())?;
    let p = Profiles {
        profiles: profiles.to_vec(),
    };
    let text = serde_yaml::to_string(&p).map_err(|e| e.to_string())?;
    std::fs::write(presets_path(), text).map_err(|e| e.to_string())
}

/// Новый профиль: имя profile-N, порт port_base + count, параметры из каталога (все выключены).
pub fn new_profile(port_base: u16, count: usize) -> Profile {
    let params = crate::params::catalog()
        .iter()
        .map(|d| ParamEntry {
            flag: d.flag.to_string(),
            value: d.default.map(|s| s.to_string()),
            enabled: false,
        })
        .collect();
    Profile {
        name: format!("profile-{}", count + 1),
        model: String::new(),
        port: (port_base as usize + count) as u16,
        image: None,
        params,
        raw_args: Vec::new(),
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Шаблонный config.yaml в корне проекта парсится (camelCase-ключи).
    #[test]
    fn parse_template_config() {
        let text = std::fs::read_to_string("../config.yaml").expect("config.yaml должен существовать");
        let cfg: AppConfig = serde_yaml::from_str(&text).expect("config.yaml должен парситься");
        assert_eq!(cfg.port_base, 9722);
        assert!(cfg.image.contains("llama"));
        assert_eq!(cfg.build_args.cuda_version, "11.4.3");
    }

    /// Round-trip: сериализация → десериализация без потерь.
    #[test]
    fn config_roundtrip() {
        let cfg = default_config();
        let text = serde_yaml::to_string(&cfg).unwrap();
        let back: AppConfig = serde_yaml::from_str(&text).unwrap();
        assert_eq!(back.models_path, cfg.models_path);
        assert_eq!(back.build_args.llama_cpp_ref, cfg.build_args.llama_cpp_ref);
    }

    /// Профиль с camelCase rawArgs парсится.
    #[test]
    fn profile_yaml_roundtrip() {
        let yaml = r#"
name: test
model: model.gguf
port: 9722
params:
  - flag: "--ctx-size"
    value: "65536"
    enabled: true
rawArgs:
  - "--custom"
"#;
        let p: Profile = serde_yaml::from_str(yaml).unwrap();
        assert_eq!(p.raw_args, vec!["--custom"]);
        assert!(p.params[0].enabled);
        let back = serde_yaml::to_string(&p).unwrap();
        assert!(back.contains("rawArgs"));
    }

    /// new_profile создаёт параметры из всего каталога.
    #[test]
    fn new_profile_has_catalog_params() {
        let p = new_profile(9722, 0);
        assert_eq!(p.name, "profile-1");
        assert_eq!(p.port, 9722);
        assert_eq!(p.params.len(), crate::params::catalog().len());
        assert!(p.params.iter().all(|e| !e.enabled));
    }

    /// В каталоге есть флаги из docker-compose.v100_CUDA12.yaml.
    #[test]
    fn catalog_has_v100_compose_flags() {
        let flags: Vec<&str> = crate::params::catalog().iter().map(|d| d.flag).collect();
        for f in [
            "--top-k",
            "--reasoning-budget-message",
            "--no-reasoning-preserve",
            "--ctx-checkpoints",
            "--checkpoint-min-step",
            "--cache-ram",
        ] {
            assert!(flags.contains(&f), "в каталоге отсутствует флаг {f}");
        }
    }
}
