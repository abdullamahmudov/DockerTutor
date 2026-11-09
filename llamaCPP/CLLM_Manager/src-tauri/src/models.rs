use serde::Serialize;
use std::path::Path;

#[derive(Serialize, Clone)]
pub struct ModelInfo {
    pub name: String,
    pub size: u64,
}

/// Список *.gguf файлов в папке моделей (без подпапок).
pub fn list_models(models_path: &str) -> Result<Vec<ModelInfo>, String> {
    let dir = Path::new(models_path);
    if !dir.is_dir() {
        return Err(format!("Папка с моделями не найдена: {models_path}"));
    }
    let mut out = Vec::new();
    for entry in std::fs::read_dir(dir).map_err(|e| e.to_string())? {
        let entry = entry.map_err(|e| e.to_string())?;
        let path = entry.path();
        if !path.is_file() {
            continue;
        }
        let ext = path.extension().and_then(|e| e.to_str()).unwrap_or("");
        if !ext.eq_ignore_ascii_case("gguf") {
            continue;
        }
        let size = entry.metadata().map(|m| m.len()).unwrap_or(0);
        let name = path
            .file_name()
            .and_then(|n| n.to_str())
            .map(|s| s.to_string())
            .unwrap_or_default();
        out.push(ModelInfo { name, size });
    }
    out.sort_by(|a, b| a.name.cmp(&b.name));
    Ok(out)
}

/// Полный путь к модели + проверка существования.
pub fn model_path(models_path: &str, model: &str) -> Result<std::path::PathBuf, String> {
    // защита от выхода за пределы папки моделей
    if model.contains("..") || model.contains('/') || model.contains('\\') {
        return Err(format!("Некорректное имя модели: {model}"));
    }
    let p = Path::new(models_path).join(model);
    if !p.is_file() {
        return Err(format!("Модель не найдена: {}", p.display()));
    }
    Ok(p)
}
