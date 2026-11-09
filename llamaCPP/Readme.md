Оптимально работающие модели:
## Qwen3.8-27B-UD-Q6_K_M

### Параметры для V100

```yaml
- "--ctx-size"
- "65536"
- "-ngl"
- "99"
- "--cache-type-k"
- "q8_0"
- "--cache-type-v"
- "q8_0"
- "--keep"
- "512"
- "--batch-size"
- "512"
- "--ubatch-size"
- "256"
- "--threads"
- "12"
- "--threads-batch"
- "16"
- "--parallel"
- "1"
- "--timeout"
- "1800"
- "--cache-reuse"
- "256"
- "--poll"
- "50"
- "--flash-attn"
- "auto"
- "--fit"
- "off"
- "--reasoning"
- "off"
- "--reasoning-budget"
- "0"
- "--jinja"
- "--repeat-penalty"
- "1.1"
- "--presence-penalty"
- "0.5"
- "--temp"
- "0.0"
- "--top-p"
- "0.9"
- "--min-p"
- "0.05"
```


----- common params -----

-h,    --help, --usage                  print usage and exit
  **RU:** Выводит справку по всем параметрам и завершает работу.
--version                               show version and build info
  **RU:** Показывает версию llama.cpp и информацию о сборке (CUDA/CPU).
-cl,   --cache-list                     show list of models in cache
  **RU:** Список моделей, уже скачанных в кэш HuggingFace (для --hf-repo).
--completion-bash                       print source-able bash completion script for llama.cpp
  **RU:** Скрипт автодополнения команд для bash. Для Windows/PowerShell бесполезен.
-t,    --threads N                      number of CPU threads to use during generation (default: -1)
                                        (env: LLAMA_ARG_THREADS)
  **RU:** Число потоков CPU при ГЕНЕРАЦИИ (по токену, узкое место — память).
  **RU:** -1 = авто. Ставьте ~числу ФИЗИЧЕСКИХ ядер; больше — просадка от переключений.
  **RU:** 12 — проверено для V100 (см. конфигурацию выше).
-tb,   --threads-batch N                number of threads to use during batch and prompt processing (default:
                                        same as --threads)
  **RU:** Потоки при обработке БАТЧЕЙ (prefill промпта, узкое место — вычисления).
  **RU:** По умолчанию = --threads. Можно ставить выше (число логических ядер/SMT): 16 — проверено.
-C,    --cpu-mask M                     CPU affinity mask: arbitrarily long hex. Complements cpu-range
                                        (default: "")
  **RU:** Маска привязки потоков к конкретным ядрам CPU (hex). Пример: 0x3F = ядра 0–5.
  **RU:** Полезно изолировать llama.cpp от других процессов; обычно не нужен.
-Cr,   --cpu-range lo-hi                range of CPUs for affinity. Complements --cpu-mask
  **RU:** Диапазон ядер CPU для привязки (например, 0-7). Альтернатива --cpu-mask.
--cpu-strict <0|1>                      use strict CPU placement (default: 0)
  **RU:** Строгая привязка потоков к ядрам (без миграции). 0 = выключено (по умолчанию).
--prio N                                set process/thread priority : low(-1), normal(0), medium(1), high(2),
                                        realtime(3) (default: 0)
  **RU:** Приоритет процесса/потоков. 0 = normal (по умолчанию). realtime — осторожно:
  **RU:** может "задавить" систему. Для сервера достаточно normal/high.
--poll <0...100>                        use polling level to wait for work (0 - no polling, default: 50)
  **RU:** Уровень опроса CPU при ожидании работы. Выше = меньше задержка, выше фоновая
  **RU:** загрузка CPU; 0 = спать до события. 50 (по умолчанию) — разумный баланс, проверено.
-Cb,   --cpu-mask-batch M               CPU affinity mask: arbitrarily long hex. Complements cpu-range-batch
                                        (default: same as --cpu-mask)
  **RU:** То же, что --cpu-mask, но для потоков БАТЧЕЙ (prefill). По умолчанию = --cpu-mask.
-Crb,  --cpu-range-batch lo-hi          ranges of CPUs for affinity. Complements --cpu-mask-batch
  **RU:** Диапазон ядер для потоков батчей. По умолчанию = --cpu-range.
--cpu-strict-batch <0|1>                use strict CPU placement (default: same as --cpu-strict)
  **RU:** Строгая привязка потоков батчей. По умолчанию = --cpu-strict.
--prio-batch N                          set process/thread priority : 0-normal, 1-medium, 2-high, 3-realtime
                                        (default: 0)
  **RU:** Приоритет потоков батчей. По умолчанию 0 (normal).
--poll-batch <0|1>                      use polling to wait for work (default: same as --poll)
  **RU:** Опрос для потоков батчей. По умолчанию = --poll.
-c,    --ctx-size N                     size of the prompt context (default: 0, 0 = loaded from model)
                                        (env: LLAMA_ARG_CTX_SIZE)
  **RU:** Размер контекста в токенах (история диалога + ответ). 0 = из метаданных модели.
  **RU:** Память KV-кэша растёт линейно (~150–250 МБ на 1K токенов для 27B при q8_0).
  **RU:** 65536 — проверено для Qwen3.8-27B на V100 (см. выше); 131072 — агрессивно;
  **RU:** 32768 — безопасный минимум для обычного чата.
-n,    --predict, --n-predict N         number of tokens to predict (default: -1, -1 = infinity)
                                        (env: LLAMA_ARG_N_PREDICT)
  **RU:** Максимум токенов в ОДНОМ ответе. -1 = без лимита (до stop-токена/конца контекста).
  **RU:** Для агентов лучше ограничить (напр. 2048–4096), чтобы не "сжигать" контекст.
-b,    --batch-size N                   logical maximum batch size (default: 2048)
                                        (env: LLAMA_ARG_BATCH)
  **RU:** Логический батч при обработке промпта (prefill). Чем больше — тем быстрее prefill,
  **RU:** тем выше пик памяти. 512 — проверено для 27B на V100; 1024 — если хватает памяти.
-ub,   --ubatch-size N                  physical maximum batch size (default: 512)
                                        (env: LLAMA_ARG_UBATCH)
  **RU:** Физический микробатч (шаг вычисления). Должен быть <= batch-size.
  **RU:** 256 — проверено для V100 (экономит память); 512 — стандарт.
--keep N                                number of tokens to keep from the initial prompt (default: 0, -1 =
                                        all)
  **RU:** Сколько первых токенов промпта держать при переполнении контекста (остальное
  **RU:** сдвигается/выбрасывается — "скользящее окно"). 512 — разумно (системный промпт
  **RU:** сохраняется); 0 (по умолчанию) — ничего не держать; -1 = всё.
--swa-full                              use full-size SWA cache (default: false)
                                        [(more
                                        info)](https://github.com/ggml-org/llama.cpp/pull/13194#issuecomment-2868343055)
                                        (env: LLAMA_ARG_SWA_FULL)
  **RU:** Полный KV-кэш для моделей со скользящим окном внимания (SWA). По умолчанию кэш
  **RU:** SWA-слоёв ограничен окном — экономия памяти. Включать только если модель/задача
  **RU:** требует полного контекста в SWA-слоях (больше памяти).
-fa,   --flash-attn [on|off|auto]       set Flash Attention use ('on', 'off', or 'auto', default: 'auto')
                                        (env: LLAMA_ARG_FLASH_ATTN)
  **RU:** Flash Attention — экономия памяти и ускорение attention. ВАЖНО: требует GPU с
  **RU:** compute capability >= 8.0 (Ampere/A100/H100...). V100 = Volta (sm_70) — НЕ
  **RU:** поддерживается. 'auto' (по умолчанию) — безопасное значение: само отключится.
  **RU:** 'on' на V100 — бессмысленно (ошибка/предупреждение).
--perf, --no-perf                       whether to enable internal libllama performance timings (default:
                                        false)
                                        (env: LLAMA_ARG_PERF)
  **RU:** Внутренняя статистика производительности (время по этапам). По умолчанию выключено;
  **RU:** включать при отладке/бенчмарках.
-e,    --escape, --no-escape            whether to process escapes sequences (\n, \r, \t, \', \", \\)
                                        (default: true)
  **RU:** Обработка escape-последовательностей в промпте. По умолчанию true — оставлять.
--rope-scaling {none,linear,yarn}       RoPE frequency scaling method, defaults to linear unless specified by
                                        the model
                                        (env: LLAMA_ARG_ROPE_SCALING_TYPE)
  **RU:** Метод масштабирования частот RoPE (позиционные кодировки) для расширения
  **RU:** контекста. По умолчанию linear (или как указано в модели). yarn — для сильного
  **RU:** расширения (см. --yarn-*). none — не масштабировать.
--rope-scale N                          RoPE context scaling factor, expands context by a factor of N
                                        (env: LLAMA_ARG_ROPE_SCALE)
  **RU:** Коэффициент расширения контекста (xN). Использовать ТОЛЬКО если модель обучена
  **RU:** на меньшем контексте, а нужен больший. Для нативных 128K-моделей (Qwen3.8)
  **RU:** не нужен — ставьте только при ошибках/деградации на длинном ctx.
--rope-freq-base N                      RoPE base frequency, used by NTK-aware scaling (default: loaded from
                                        model)
                                        (env: LLAMA_ARG_ROPE_FREQ_BASE)
  **RU:** Базовая частота RoPE для NTK-aware масштабирования. По умолчанию из модели —
  **RU:** менять только при ручной настройке NTK (экспериментально).
--rope-freq-scale N                     RoPE frequency scaling factor, expands context by a factor of 1/N
                                        (env: LLAMA_ARG_ROPE_FREQ_SCALE)
  **RU:** Масштаб частот RoPE (контекст расширяется в 1/N раз). Тоже только для
  **RU:** экспериментов с NTK; по умолчанию не трогать.
--yarn-orig-ctx N                       YaRN: original context size of model (default: 0 = model training
                                        context size)
                                        (env: LLAMA_ARG_YARN_ORIG_CTX)
  **RU:** YaRN: исходный размер контекста модели (0 = из метаданных). Задавать, если
  **RU:** расширяете контекст через YaRN и метаданные некорректны.
--yarn-ext-factor N                     YaRN: extrapolation mix factor (default: -1.00, 0.0 = full
                                        interpolation)
                                        (env: LLAMA_ARG_YARN_EXT_FACTOR)
  **RU:** YaRN: фактор смешения экстраполяции (0 = полная интерполяция). -1 = значение
  **RU:** из метаданных модели. Настройка "вслепую" не рекомендуется.
--yarn-attn-factor N                    YaRN: scale sqrt(t) or attention magnitude (default: -1.00)
                                        (env: LLAMA_ARG_YARN_ATTN_FACTOR)
  **RU:** YaRN: масштабирование величины attention. -1 = из метаданных. Экспериментально.
--yarn-beta-slow N                      YaRN: high correction dim or alpha (default: -1.00)
                                        (env: LLAMA_ARG_YARN_BETA_SLOW)
  **RU:** YaRN: параметр "медленных" измерений коррекции (alpha). -1 = из метаданных.
--yarn-beta-fast N                      YaRN: low correction dim or beta (default: -1.00)
                                        (env: LLAMA_ARG_YARN_BETA_FAST)
  **RU:** YaRN: параметр "быстрых" измерений коррекции (beta). -1 = из метаданных.
  **RU:** Итог по RoPE/YaRN: для моделей с нативным длинным контекстом (Qwen3.8 27B)
  **RU:** все эти параметры НЕ ТРОГАТЬ — расширение "на коленке" деградирует качество.
-kvo,  --kv-offload, -nkvo, --no-kv-offload
                                        whether to enable KV cache offloading (default: enabled)
                                        (env: LLAMA_ARG_KV_OFFLOAD)
  **RU:** Выгрузка части KV-кэша на GPU. По умолчанию вкл. — правильно для V100:
  **RU:** разгружает RAM. --no-kv-offload — только если GPU-память критически мала.
--repack, -nr, --no-repack              whether to enable weight repacking (default: enabled)
                                        (env: LLAMA_ARG_REPACK)
  **RU:** Пересборка весов в оптимальный для GPU формат (ускорение вычислений).
  **RU:** По умолчанию вкл. — оставить. --no-repack — только если репак ломает загрузку.
--no-host                               bypass host buffer allowing extra buffers to be used
                                        (env: LLAMA_ARG_NO_HOST)
  **RU:** Отключить хост-буфер (доп. буферы на CPU). Экспериментальный флаг; по
  **RU:** умолчанию не использовать.
-ctk,  --cache-type-k TYPE              KV cache data type for K
                                        allowed values: f32, f16, bf16, q8_0, q4_0, q4_1, iq4_nl, q5_0, q5_1
                                        (default: f16)
                                        (env: LLAMA_ARG_CACHE_TYPE_K)
  **RU:** Тип данных KV-кэша для K. f16 (по умолчанию) — точно, но кэш в 2 раза больше.
  **RU:** q8_0 — почти без потери качества, кэш ~в 2 раза меньше: РЕКОМЕНДУЕТСЯ для
  **RU:** больших ctx на V100 (проверено). q4_0/q4_1 — ещё меньше памяти, но заметная
  **RU:** деградация на длинном контексте (лучше не использовать).
-ctv,  --cache-type-v TYPE              KV cache data type for V
                                        allowed values: f32, f16, bf16, q8_0, q4_0, q4_1, iq4_nl, q5_0, q5_1
                                        (default: f16)
                                        (env: LLAMA_ARG_CACHE_TYPE_V)
  **RU:** То же, что --cache-type-k, но для V. Рекомендации те же: q8_0 — оптимум.
-dt,   --defrag-thold N                 KV cache defragmentation threshold (DEPRECATED)
                                        (env: LLAMA_ARG_DEFRAG_THOLD)
  **RU:** Порог дефрагментации KV-кэша. УСТАРЕЛ (DEPRECATED) — не использовать.
-lm,   --load-mode MODE                 model loading mode (default: auto)
                                        - auto: mmap, unless a device does not support it
                                        - none: no special loading mode
                                        - mmap: memory-map model (if mmap disabled, slower load but may reduce
                                        pageouts if not using mlock)
                                        - mlock: force system to keep model in RAM rather than swapping or
  **RU:** (описание в файле оборвано; полный список режимов:)
  **RU:**   auto      — mmap, если устройство его поддерживает (по умолчанию);
  **RU:**   none      — без особого режима загрузки;
  **RU:**   mmap      — memory-map модели (медленнее загрузка, но меньше pageout'ы);
  **RU:**   mlock     — запрет ОС выгружать модель в swap (убирает "заикания", но сразу
  **RU:**             резервирует всю ОЗУ под модель);
  **RU:**   mmap+mlock — mmap + mlock;  dio — Direct I/O.
  **RU:** Для гибридной работы модель RAM+VRAM (наша V100) mlock/mmap+mlock полезен,
  **RU:** если ОЗУ хватает с запасом. Это замена старого флага --mlock.
  **RU:**
  **RU:** Примечание: блок выше — выдержка из `llama-server --help` (в исходнике он
  **RU:** оборван на --load-mode). Полный актуальный список параметров:
  **RU:** `docker compose run --rm llama --help` или tools/server/README.md в репозитории
  **RU:** ggml-org/llama.cpp.
