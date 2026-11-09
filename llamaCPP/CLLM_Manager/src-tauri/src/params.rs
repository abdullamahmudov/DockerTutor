use serde::Serialize;

/// Описание одного параметра llama-server для UI.
#[derive(Serialize, Clone)]
pub struct ParamDef {
    pub flag: &'static str,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub short: Option<&'static str>,
    pub name: &'static str,
    pub group: &'static str,
    pub kind: &'static str, // int | float | str | bool | select
    pub options: Vec<&'static str>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub default: Option<&'static str>,
    pub help: &'static str,
}

const KV_TYPES: &[&str] = &["f32", "f16", "bf16", "q8_0", "q4_0", "q4_1", "iq4_nl", "q5_0", "q5_1"];
const FA_OPTS: &[&str] = &["auto", "on", "off"];
const LOAD_MODES: &[&str] = &["auto", "none", "mmap", "mlock", "mmap+mlock", "dio"];
const ROPE_OPTS: &[&str] = &["none", "linear", "yarn"];
const ON_OFF: &[&str] = &["off", "on"];

fn sel(options: &'static [&'static str]) -> Vec<&'static str> {
    options.to_vec()
}

fn no_opts() -> Vec<&'static str> {
    Vec::new()
}

/// Каталог параметров (источники: Readme.md + docker-compose.cmake_v100.yaml).
pub fn catalog() -> Vec<ParamDef> {
    let mut v = vec![
        // ---------- Контекст и память ----------
        ParamDef {
            flag: "--ctx-size",
            short: Some("-c"),
            name: "Размер контекста (токенов)",
            group: "Контекст и память",
            kind: "int",
            options: no_opts(),
            default: Some("65536"),
            help: "Размер контекста в токенах (история диалога + ответ). 0 = из метаданных модели. \
Память KV-кэша растёт линейно (~150–250 МБ на 1K токенов для 27B при q8_0). \
65536 — проверено для Qwen3.8-27B на V100; 131072 — агрессивно; 32768 — безопасный минимум.",
        },
        ParamDef {
            flag: "--n-predict",
            short: Some("-n"),
            name: "Макс. токенов в ответе",
            group: "Контекст и память",
            kind: "int",
            options: no_opts(),
            default: Some("-1"),
            help: "Максимум токенов в ОДНОМ ответе. -1 = без лимита (до stop-токена/конца контекста). \
Для агентов лучше ограничить (2048–4096), чтобы не «сжигать» контекст.",
        },
        ParamDef {
            flag: "--keep",
            short: None,
            name: "Токенов держать из промпта",
            group: "Контекст и память",
            kind: "int",
            options: no_opts(),
            default: Some("512"),
            help: "Сколько первых токенов промпта держать при переполнении контекста (остальное выбрасывается). \
512 — разумно (системный промпт сохраняется); 0 — ничего; -1 = всё.",
        },
        ParamDef {
            flag: "--swa-full",
            short: None,
            name: "Полный SWA-кэш",
            group: "Контекст и память",
            kind: "bool",
            options: no_opts(),
            default: None,
            help: "Полноразмерный KV-кэш для моделей со скользящим окном внимания (SWA). \
По умолчанию выключено (кэш SWA-слоёв ограничен окном — экономия памяти).",
        },
        ParamDef {
            flag: "--flash-attn",
            short: Some("-fa"),
            name: "Flash Attention",
            group: "Контекст и память",
            kind: "select",
            options: sel(FA_OPTS),
            default: Some("auto"),
            help: "Flash Attention — экономия памяти и ускорение attention. Требует GPU с compute capability >= 8.0. \
V100 = Volta (sm_70) — НЕ поддерживается; 'auto' — безопасное значение (само отключится).",
        },
        ParamDef {
            flag: "--cache-type-k",
            short: Some("-ctk"),
            name: "Тип KV-кэша K",
            group: "Контекст и память",
            kind: "select",
            options: sel(KV_TYPES),
            default: Some("q8_0"),
            help: "Тип данных KV-кэша для K. q8_0 — почти без потери качества, кэш ~в 2 раза меньше: \
РЕКОМЕНДУЕТСЯ для больших ctx на V100 (проверено). f16 — точно, но кэш в 2 раза больше.",
        },
        ParamDef {
            flag: "--cache-type-v",
            short: Some("-ctv"),
            name: "Тип KV-кэша V",
            group: "Контекст и память",
            kind: "select",
            options: sel(KV_TYPES),
            default: Some("q8_0"),
            help: "То же, что --cache-type-k, но для V. Рекомендации те же: q8_0 — оптимум.",
        },
        ParamDef {
            flag: "--load-mode",
            short: Some("-lm"),
            name: "Режим загрузки модели",
            group: "Контекст и память",
            kind: "select",
            options: sel(LOAD_MODES),
            default: Some("auto"),
            help: "Режим загрузки модели. mlock — запрет ОС выгружать модель в swap (убирает «заикания», \
но сразу резервирует всю ОЗУ). auto — mmap, если устройство поддерживает. Замена старого --mlock.",
        },
    ];
    v.extend(gpu_cpu());
    v.extend(sampling());
    v.extend(server_misc());
    v
}

fn gpu_cpu() -> Vec<ParamDef> {
    vec![
        // ---------- GPU / VRAM ----------
        ParamDef {
            flag: "-ngl",
            short: None,
            name: "Слоёв на GPU",
            group: "GPU / VRAM",
            kind: "int",
            options: no_opts(),
            default: Some("99"),
            help: "Число слоёв модели, выгружаемых на GPU. 99 = все слои. \
Если VRAM не хватает — часть модели останется в RAM (гибридный режим).",
        },
        ParamDef {
            flag: "--kv-offload",
            short: Some("-kvo"),
            name: "Выгрузка KV-кэша на GPU",
            group: "GPU / VRAM",
            kind: "bool",
            options: no_opts(),
            default: None,
            help: "Выгрузка части KV-кэша на GPU. По умолчанию вкл. — разгружает RAM. \
Если снять чекбокс, флаг не передаётся (останется дефолтное поведение).",
        },
        ParamDef {
            flag: "--repack",
            short: None,
            name: "Репак весов",
            group: "GPU / VRAM",
            kind: "bool",
            options: no_opts(),
            default: None,
            help: "Пересборка весов в оптимальный для GPU формат (ускорение вычислений). По умолчанию вкл.",
        },
        // ---------- Потоки CPU ----------
        ParamDef {
            flag: "--threads",
            short: Some("-t"),
            name: "Потоки CPU (генерация)",
            group: "Потоки CPU",
            kind: "int",
            options: no_opts(),
            default: Some("12"),
            help: "Число потоков CPU при ГЕНЕРАЦИИ (по токену, узкое место — память). \
-1 = авто. Ставьте ~числу ФИЗИЧЕСКИХ ядер. 12 — проверено для V100.",
        },
        ParamDef {
            flag: "--threads-batch",
            short: Some("-tb"),
            name: "Потоки CPU (батчи)",
            group: "Потоки CPU",
            kind: "int",
            options: no_opts(),
            default: Some("16"),
            help: "Потоки при обработке БАТЧЕЙ (prefill промпта, узкое место — вычисления). \
Можно ставить выше --threads (число логических ядер/SMT). 16 — проверено.",
        },
        ParamDef {
            flag: "--poll",
            short: None,
            name: "Уровень опроса CPU",
            group: "Потоки CPU",
            kind: "int",
            options: no_opts(),
            default: Some("50"),
            help: "Уровень опроса CPU при ожидании работы (0–100). Выше = меньше задержка, выше фоновая \
загрузка CPU; 0 = спать до события. 50 — разумный баланс.",
        },
        ParamDef {
            flag: "--prio",
            short: None,
            name: "Приоритет процесса",
            group: "Потоки CPU",
            kind: "int",
            options: no_opts(),
            default: Some("0"),
            help: "Приоритет процесса/потоков: -1 low, 0 normal (по умолчанию), 1 medium, 2 high, 3 realtime.",
        },
        ParamDef {
            flag: "--cpu-mask",
            short: Some("-C"),
            name: "Маска CPU (hex)",
            group: "Потоки CPU",
            kind: "str",
            options: no_opts(),
            default: None,
            help: "Маска привязки потоков к ядрам CPU (hex). Пример: 0x3F = ядра 0–5. \
Полезно изолировать llama.cpp от других процессов.",
        },
        ParamDef {
            flag: "--cpu-range",
            short: Some("-Cr"),
            name: "Диапазон CPU",
            group: "Потоки CPU",
            kind: "str",
            options: no_opts(),
            default: None,
            help: "Диапазон ядер CPU для привязки, например 0-7.",
        },
    ]
}


fn sampling() -> Vec<ParamDef> {
    vec![
        // ---------- Генерация (sampling) ----------
        ParamDef {
            flag: "--temp",
            short: None,
            name: "Температура",
            group: "Генерация (sampling)",
            kind: "float",
            options: no_opts(),
            default: Some("0.0"),
            help: "Температура выборки. 0 = жадная (детерминированная) генерация — для агентов.",
        },
        ParamDef {
            flag: "--top-p",
            short: None,
            name: "Top-p",
            group: "Генерация (sampling)",
            kind: "float",
            options: no_opts(),
            default: Some("0.9"),
            help: "Ядровое (nucleus) сэмплирование: порог накопленной вероятности.",
        },
        ParamDef {
            flag: "--min-p",
            short: None,
            name: "Min-p",
            group: "Генерация (sampling)",
            kind: "float",
            options: no_opts(),
            default: Some("0.05"),
            help: "Минимальная вероятность токена относительно самого вероятного.",
        },
        ParamDef {
            flag: "--repeat-penalty",
            short: None,
            name: "Штраф за повторение",
            group: "Генерация (sampling)",
            kind: "float",
            options: no_opts(),
            default: Some("1.1"),
            help: "Штраф за повторение токенов. >1 — подавляет повторы; 1.1 — проверено.",
        },
        ParamDef {
            flag: "--presence-penalty",
            short: None,
            name: "Presence penalty",
            group: "Генерация (sampling)",
            kind: "float",
            options: no_opts(),
            default: Some("0.5"),
            help: "Штраф за уже встречавшиеся токены (не зависит от частоты).",
        },
        ParamDef {
            flag: "--batch-size",
            short: Some("-b"),
            name: "Batch size (логический)",
            group: "Генерация (sampling)",
            kind: "int",
            options: no_opts(),
            default: Some("512"),
            help: "Логический батч при обработке промпта (prefill). Чем больше — тем быстрее prefill, \
тем выше пик памяти. 512 — проверено для 27B на V100.",
        },
        ParamDef {
            flag: "--ubatch-size",
            short: Some("-ub"),
            name: "Ubatch size (физический)",
            group: "Генерация (sampling)",
            kind: "int",
            options: no_opts(),
            default: Some("256"),
            help: "Физический микробатч (шаг вычисления). Должен быть <= batch-size. \
256 — проверено для V100 (экономит память).",
        },
        ParamDef {
            flag: "--cache-reuse",
            short: None,
            name: "Переиспользование KV-кэша",
            group: "Генерация (sampling)",
            kind: "int",
            options: no_opts(),
            default: Some("256"),
            help: "Число токенов для переиспользования из KV-кэша между запросами.",
        },
        ParamDef {
            flag: "--fit",
            short: None,
            name: "Fit",
            group: "Генерация (sampling)",
            kind: "select",
            options: sel(ON_OFF),
            default: Some("off"),
            help: "Режим 'fit'. off — не менять поведение.",
        },
        ParamDef {
            flag: "--reasoning",
            short: None,
            name: "Reasoning",
            group: "Генерация (sampling)",
            kind: "select",
            options: sel(ON_OFF),
            default: Some("off"),
            help: "Режим рассуждений (reasoning). off — выключен.",
        },
        ParamDef {
            flag: "--reasoning-budget",
            short: None,
            name: "Бюджет reasoning",
            group: "Генерация (sampling)",
            kind: "int",
            options: no_opts(),
            default: Some("0"),
            help: "Бюджет токенов на рассуждения. 0 = без бюджета.",
        },
        ParamDef {
            flag: "--jinja",
            short: None,
            name: "Jinja-шаблоны",
            group: "Генерация (sampling)",
            kind: "bool",
            options: no_opts(),
            default: None,
            help: "Включить обработку Jinja-шаблонов (chat template из метаданных модели).",
        },
    ]
}


fn server_misc() -> Vec<ParamDef> {
    vec![
        // ---------- RoPE / YaRN ----------
        ParamDef {
            flag: "--rope-scaling",
            short: None,
            name: "Метод RoPE scaling",
            group: "RoPE / YaRN",
            kind: "select",
            options: sel(ROPE_OPTS),
            default: None,
            help: "Метод масштабирования частот RoPE. Для моделей с нативным длинным контекстом \
(Qwen3.8) НЕ ТРОГАТЬ — расширение «на коленке» деградирует качество.",
        },
        ParamDef {
            flag: "--rope-scale",
            short: None,
            name: "RoPE scale (xN)",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "Коэффициент расширения контекста (xN). Только если модель обучена на меньшем контексте.",
        },
        ParamDef {
            flag: "--rope-freq-base",
            short: None,
            name: "RoPE freq base",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "Базовая частота для NTK-aware scaling. По умолчанию из модели — менять только при ручной настройке NTK.",
        },
        ParamDef {
            flag: "--rope-freq-scale",
            short: None,
            name: "RoPE freq scale (1/N)",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "Масштаб частот (контекст расширяется в 1/N раз). Экспериментально.",
        },
        ParamDef {
            flag: "--yarn-orig-ctx",
            short: None,
            name: "YaRN: исходный ctx",
            group: "RoPE / YaRN",
            kind: "int",
            options: no_opts(),
            default: None,
            help: "YaRN: исходный размер контекста модели (0 = из метаданных).",
        },
        ParamDef {
            flag: "--yarn-ext-factor",
            short: None,
            name: "YaRN: ext factor",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "YaRN: фактор смешения экстраполяции. -1 = из метаданных модели.",
        },
        ParamDef {
            flag: "--yarn-attn-factor",
            short: None,
            name: "YaRN: attn factor",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "YaRN: масштаб величины attention. -1 = из метаданных. Экспериментально.",
        },
        ParamDef {
            flag: "--yarn-beta-slow",
            short: None,
            name: "YaRN: beta slow",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "YaRN: alpha «медленных» измерений коррекции. -1 = из метаданных.",
        },
        ParamDef {
            flag: "--yarn-beta-fast",
            short: None,
            name: "YaRN: beta fast",
            group: "RoPE / YaRN",
            kind: "float",
            options: no_opts(),
            default: None,
            help: "YaRN: beta «быстрых» измерений коррекции. -1 = из метаданных.",
        },
        // ---------- Сервер ----------
        ParamDef {
            flag: "--timeout",
            short: None,
            name: "Таймаут запроса (сек)",
            group: "Сервер",
            kind: "int",
            options: no_opts(),
            default: Some("1800"),
            help: "Таймаут ожидания ответа (секунды). 1800 — проверено для длинных генераций.",
        },
        ParamDef {
            flag: "--parallel",
            short: None,
            name: "Параллельные слоты",
            group: "Сервер",
            kind: "int",
            options: no_opts(),
            default: Some("1"),
            help: "Число параллельных слотов обработки. 1 — по умолчанию (меньше расход памяти).",
        },
        ParamDef {
            flag: "--api-key",
            short: None,
            name: "API-ключ",
            group: "Сервер",
            kind: "str",
            options: no_opts(),
            default: None,
            help: "API-ключ для доступа к серверу. Пусто — без аутентификации.",
        },
        // ---------- Прочее ----------
        ParamDef {
            flag: "--perf",
            short: None,
            name: "Статистика производительности",
            group: "Прочее",
            kind: "bool",
            options: no_opts(),
            default: None,
            help: "Внутренняя статистика производительности (время по этапам). Для отладки/бенчмарков.",
        },
        ParamDef {
            flag: "--escape",
            short: Some("-e"),
            name: "Escape-последовательности",
            group: "Прочее",
            kind: "bool",
            options: no_opts(),
            default: None,
            help: "Обработка escape-последовательностей в промпте. По умолчанию включено.",
        },
    ]
}

