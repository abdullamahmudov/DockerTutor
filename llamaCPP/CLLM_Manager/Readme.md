# CLLM Manager

## 1. Описание

**CLLM Manager** — десктопное приложение (Windows) для управления локальными LLM-серверами
на базе **llama.cpp**, запущенными в **Docker-контейнерах** (GPU: NVIDIA V100, CUDA).

Приложение решает три задачи:

1. **Профили запуска** — наборы параметров `llama-server` (контекст, KV-кэш, потоки,
   sampling и др.) с привязкой к модели и порту. Запуск/остановка контейнеров в один
   клик, live-логи контейнера прямо в UI.
2. **Загрузка моделей** — скачивание `.gguf`-файлов с HuggingFace (в т.ч. gated-репозитории
   по токену) с прогрессом, скоростью и возобновлением на обрыве (resume).
3. **Сборка образа** — `docker build` из вашего `V100.Dockerfile` с настраиваемыми
   build-аргументами (CUDA, Ubuntu, референс llama.cpp, compute capability) и стримом
   лога сборки.

Ключевой принцип: **выключенный в профиле параметр не попадает в команду запуска** —
в контейнер передаётся только то, что реально включено, плюс произвольные raw-аргументы.

## 2. Шаги для запуска (dev-режим)

Требования:

- **Node.js** ≥ 18
- **Rust** (stable toolchain) — `cargo` должен быть доступен в `PATH`
  (например, `%USERPROFILE%\.cargo\bin`)
- **Docker Desktop** с поддержкой GPU (`--gpus all`)
- **WebView2** (в Windows 10/11 уже встроен)

```powershell
cd llamaCPP\CLLM_Manager

# 1. Зависимости фронтенда
npm install

# 2. Запуск в dev-режиме (vite + tauri, hot-reload)
npm run tauri dev
```

Альтернатива — запуск **отдельным отсоединённым процессом** (переживает закрытие
терминала; лог пишется в `tauri-dev.log`):

```powershell
# из корня проекта llamaCPP\CLLM_Manager
powershell -ExecutionPolicy Bypass -File .\run-dev.ps1
```

Остановка dev-сессии:

```powershell
# Только окно приложения:
Stop-Process -Name 'cllm-manager'

# Вся сессия (app + vite + cargo):
Get-Process cllm-manager, node -ErrorAction SilentlyContinue | Stop-Process
```

При первом запуске проверьте/отредактируйте `config.yaml` в корне проекта
(путь к моделям, Dockerfile, имя образа, базовый порт, HF-токен).

Полезные команды:

```powershell
npx tsc --noEmit                              # проверка типов фронтенда
cargo test --manifest-path src-tauri\Cargo.toml   # юнит-тесты бэкенда
```

> ⚠️ Если `npm run tauri dev` выдаёт `failed to run 'cargo metadata' ... program not found` —
> `cargo` не в `PATH`. Добавьте `%USERPROFILE%\.cargo\bin` в переменную окружения.

## 3. Шаги для сборки (release)

```powershell
cd llamaCPP\CLLM_Manager

npm install
npm run tauri build
```

Что происходит:

1. `npm run build` → `tsc && vite build` — фронтенд собирается в `dist/`;
2. `cargo build --release` — Rust-бэкенд (профиль release: LTO, `opt-level = "s"`, strip);
3. Генерация установщика **NSIS** в `src-tauri/target/release/bundle/nsis/`.

Итоговые артефакты:

| Файл | Назначение |
|---|---|
| `src-tauri\target\release\bundle\nsis\*.exe` | Установщик для Windows |
| `src-tauri\target\release\cllm-manager.exe` | Готовый бинарник (portable) |

В release-сборке `config.yaml` и `presets.yaml` ищутся **рядом с exe**
(в dev — в корне проекта). Каталог можно переопределить переменной окружения
`CLLM_CONFIG_DIR`.
## 4. Стек

| Слой | Технологии |
|---|---|
| Desktop-оболочка | **Tauri 2** (Rust-бэкенд + WebView) |
| Бэкенд | **Rust**: `tokio` (async, процессы), `reqwest` + `futures-util` (HTTP/стриминг), `serde`/`serde_yaml`/`serde_json` (конфиги), `thiserror` |
| Фронтенд | **React 18** + **TypeScript 5** (strict) |
| Сборка фронтенда | **Vite 6**, `@vitejs/plugin-react` |
| Стили | **Tailwind CSS v4** (`@tailwindcss/vite`, кастомные классы через `@utility`) |
| Интеграции | **Docker CLI** (run/stop/logs/build), **HuggingFace API** (список файлов, скачивание с `Range`-резюмом) |
| Конфигурация | YAML: `config.yaml`, `presets.yaml` |
| Дистрибуция | **NSIS** (Windows) |

## 5. Архитектура

```
┌──────────────────────────────────────────────────────────────┐
│  WebView (React + TS)                                        │
│  ┌───────────────┐  ┌───────────────┐  ┌─────────────────┐   │
│  │  ProfilesTab  │  │     HfTab     │  │   SettingsTab   │   │
│  │ редактор      │  │ HF: список    │  │ config.yaml +   │   │
│  │ профилей,     │  │ .gguf,        │  │ сборка образа   │   │
│  │ live-логи     │  │ скачивание    │  │                 │   │
│  └───────┬───────┘  └───────┬───────┘  └────────┬────────┘   │
│          └───────────┬──────┴───────────────────┘            │
│                      │ invoke() (Tauri commands)             │
│                      │ listen(): container-log / build-log / │
│                      │             hf-progress               │
├──────────────────────▼───────────────────────────────────────┤
│  Rust core (src-tauri/src)                                   │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌─────────────────┐  │
│  │  config  │ │  params  │ │  models  │ │       hf        │  │
│  │ YAML:    │ │ каталог  │ │ .gguf    │ │ HF API, resume, │  │
│  │ cfg +    │ │ ~35      │ │ скан     │ │ стрим прогресса │  │
│  │ profiles │ │ флагов   │ │          │ │                 │  │
│  └────┬─────┘ │ с help   │ └──────────┘ └─────────────────┘  │
│       │       └────┬─────┘                                   │
│       ▼            ▼                                          │
│  ┌─────────────────────────────────────┐                      │
│  │              docker                 │                      │
│  │ run/stop/list, live-логи            │──► Docker CLI        │
│  │ (docker logs -f), docker build      │   (внешние процессы) │
│  └─────────────────────────────────────┘                      │
└──────────────────────────────────────────────────────────────┘
```

### Модули бэкенда (`src-tauri/src/`)

| Модуль | Ответственность |
|---|---|
| `lib.rs` | Точка входа Tauri, регистрация 16 команд (`get_config`, `start_container`, `hf_download`, …) |
| `config.rs` | Чтение/запись `config.yaml` и `presets.yaml`; фабрика нового профиля; юнит-тесты |
| `params.rs` | Каталог параметров `llama-server`: группы, типы (int/float/str/bool/select), значения по умолчанию и русские подсказки (источники — Readme.md проекта и `docker-compose.cmake_v100.yaml`) |
| `docker.rs` | Запуск/остановка контейнеров (`cllm-mgr-<профиль>`), проверка порта и модели, реестр процессов `docker logs -f` со стримингом строк в UI, `docker build` с параллельным чтением stdout/stderr |
| `hf.rs` | Список `.gguf` репозитория через HF API, скачивание с заголовком `Range` (resume), атомарный флаг отмены, событие `hf-progress` |
| `models.rs` | Сканирование папки моделей (`*.gguf`), защита от path traversal |

### Ключевые механизмы

- **Профили** (`presets.yaml`) — `name`, `model`, `port`, опциональный `image`,
  список параметров (`flag`/`value`/`enabled`) и `rawArgs`. При запуске Rust
  собирает аргументы `llama-server` только из включённых позиций.
- **Live-логи** — на каждый просматриваемый профиль поднимается процесс
  `docker logs -f --tail 300`; строки уходят в UI событием `container-log`,
  при остановке профиля tailer убивается.
- **Скачивание HF** — прогресс эмитится каждые ~400 мс (`received`/`total`/`speed`),
  обрыв продолжается с последнего байта; отмена — флаг `AtomicBool`.
- **Синхронизация Rust↔TS** — все DTO сериализуются в `camelCase`
  (`#[serde(rename_all = "camelCase")]`), типы зеркальны в `src/types.ts`.
- **Каталог конфигов** — в dev: корень проекта (ищется вверх от exe по
  `package.json` + `src-tauri`); в release: папка с exe; override — `CLLM_CONFIG_DIR`.

### Файлы конфигурации

```yaml
# config.yaml
modelsPath: D:\...\llamaCPP\models        # папка .gguf-моделей
dockerfile: D:\...\llamaCPP\V100.Dockerfile
image: llama-cpp-v100:cuda11.4.3          # образ по умолчанию
portBase: 9722                            # порт нового профиля = portBase + N
# hfToken: hf_xxx...                      # для gated-репозиториев HF
buildArgs:
  cudaVersion: "11.4.3"
  ubuntuVersion: "20.04"
  llamaCppRef: b5900
  cudaArch: "70"                          # V100 = Volta (sm_70)
```

