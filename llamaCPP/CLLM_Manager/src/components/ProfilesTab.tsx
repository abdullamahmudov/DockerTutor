import { useEffect, useMemo, useRef, useState } from "react";
import type {
  AppConfig,
  ContainerStatus,
  ModelInfo,
  ParamDef,
  ParamEntry,
  Profile,
} from "../types";

interface Props {
  config: AppConfig;
  paramDefs: ParamDef[];
  models: ModelInfo[];
  profiles: Profile[];
  statuses: ContainerStatus[];
  selected: string | null;
  selectedStatus: ContainerStatus | undefined;
  viewLogs: string[];
  onSelect: (name: string) => void;
  onNew: () => Promise<void>;
  onDelete: (name: string) => Promise<void>;
  onSave: (p: Profile) => Promise<void>;
  onStart: (name: string) => Promise<void>;
  onStop: (name: string) => Promise<void>;
  onRefreshModels: () => Promise<void>;
}

function fmtSize(bytes: number): string {
  return `${(bytes / 1024 ** 3).toFixed(1)} ГБ`;
}

export default function ProfilesTab(props: Props) {
  const { profiles, statuses, selected, paramDefs, models, viewLogs } = props;
  const [draft, setDraft] = useState<Profile | null>(null);
  const [busy, setBusy] = useState(false);
  const logRef = useRef<HTMLDivElement>(null);

  // черновик следует за выбранным профилем
  useEffect(() => {
    const p = profiles.find((x) => x.name === selected) ?? null;
    setDraft(p ? (JSON.parse(JSON.stringify(p)) as Profile) : null);
  }, [selected, profiles]);

  // автоскролл логов
  useEffect(() => {
    if (logRef.current) logRef.current.scrollTop = logRef.current.scrollHeight;
  }, [viewLogs]);

  const handleSave = async () => {
    if (!draft) return;
    setBusy(true);
    try {
      await props.onSave(draft);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="flex h-full flex-col gap-2">
      <div className="flex min-h-0 flex-1 gap-2">
        {/* Левая панель: профили */}
        <aside className="flex w-72 shrink-0 flex-col gap-2 overflow-y-auto pr-1">
          <button className="btn-primary" onClick={() => void props.onNew()}>
            + Новый профиль
          </button>
          {profiles.map((p) => {
            const st = statuses.find((s) => s.profile === p.name);
            const running = !!st?.running;
            const dot = running ? "bg-green-500" : st ? "bg-red-500" : "bg-gray-600";
            return (
              <div
                key={p.name}
                className={`panel cursor-pointer p-3 transition-colors ${
                  selected === p.name ? "border-indigo-500" : "hover:border-gray-600"
                }`}
                onClick={() => props.onSelect(p.name)}
              >
                <div className="flex items-center gap-2">
                  <span className={`inline-block h-2.5 w-2.5 shrink-0 rounded-full ${dot}`} />
                  <span className="truncate font-semibold">{p.name}</span>
                </div>
                <div className="mt-1 truncate text-xs text-gray-400">
                  {p.model || "— модель не выбрана —"}
                </div>
                <div className="text-xs text-gray-500">порт {p.port}</div>
                <div className="mt-2 flex gap-1">
                  {running ? (
                    <button
                      className="btn-danger"
                      onClick={(e) => {
                        e.stopPropagation();
                        void props.onStop(p.name);
                      }}
                    >
                      ■ Стоп
                    </button>
                  ) : (
                    <button
                      className="btn-primary"
                      onClick={(e) => {
                        e.stopPropagation();
                        void props.onStart(p.name);
                      }}
                    >
                      ▶ Старт
                    </button>
                  )}
                  <button
                    className="btn-ghost"
                    title="Удалить профиль"
                    onClick={(e) => {
                      e.stopPropagation();
                      if (confirm(`Удалить профиль "${p.name}"?`)) void props.onDelete(p.name);
                    }}
                  >
                    🗑
                  </button>
                </div>
              </div>
            );
          })}
        </aside>

        {/* Правая панель: редактор профиля */}
        <section className="panel min-h-0 flex-1 overflow-y-auto p-4">
          {draft ? (
            <Editor
              draft={draft}
              setDraft={setDraft}
              paramDefs={paramDefs}
              models={models}
              running={!!props.selectedStatus?.running}
              originalName={selected ?? ""}
              busy={busy}
              onRefreshModels={props.onRefreshModels}
              onSave={handleSave}
            />
          ) : (
            <div className="text-gray-500">
              Выберите профиль слева или создайте новый.
            </div>
          )}
        </section>
      </div>

      {/* Панель логов */}
      <div className="panel flex h-56 shrink-0 flex-col">
        <div className="flex items-center gap-2 border-b border-gray-800 px-3 py-1.5 text-sm text-gray-400">
          Логи: <span className="font-mono text-gray-300">{selected ?? "—"}</span>
          <span className="ml-auto text-xs text-gray-600">live (docker logs -f)</span>
        </div>
        <div ref={logRef} className="min-h-0 flex-1 overflow-y-auto p-2">
          {viewLogs.length === 0 ? (
            <div className="log-line">
              Нет логов (контейнер не запущен или ещё не вывел данные)
            </div>
          ) : (
            viewLogs.map((l, i) => <div key={i} className="log-line">{l}</div>)
          )}
        </div>
      </div>
    </div>
  );
}

// ---------- Редактор профиля ----------

interface EditorProps {
  draft: Profile;
  setDraft: (fn: (d: Profile | null) => Profile | null) => void;
  paramDefs: ParamDef[];
  models: ModelInfo[];
  running: boolean;
  originalName: string;
  busy: boolean;
  onRefreshModels: () => Promise<void>;
  onSave: () => Promise<void>;
}

// «Проверенный набор V100» из docker-compose.v100_CUDA12.yaml
const V100_PRESET: Record<string, string | null> = {
  "--ctx-size": "131072",
  "-ngl": "99",
  "--cache-type-k": "q8_0",
  "--cache-type-v": "q8_0",
  "--keep": "512",
  "--batch-size": "1024",
  "--ubatch-size": "512",
  "--threads": "12",
  "--threads-batch": "16",
  "--parallel": "1",
  "--timeout": "1800",
  "--poll": "50",
  "--flash-attn": "auto",
  "--fit": "off",
  "--reasoning": "on",
  "--reasoning-budget": "2048",
  "--reasoning-budget-message": "I should stop thinking and act now.",
  "--no-reasoning-preserve": null,
  "--ctx-checkpoints": "32",
  "--checkpoint-min-step": "2048",
  "--cache-ram": "16384",
  "--jinja": null,
  "--repeat-penalty": "1.0",
  "--presence-penalty": "0.5",
  "--temp": "0.6",
  "--top-p": "0.95",
  "--top-k": "20",
  "--min-p": "0.0",
};

function Editor({
  draft,
  setDraft,
  paramDefs,
  models,
  running,
  originalName,
  busy,
  onRefreshModels,
  onSave,
}: EditorProps) {
  const groups = useMemo(() => {
    const m = new Map<string, ParamDef[]>();
    for (const d of paramDefs) {
      const arr = m.get(d.group);
      if (arr) arr.push(d);
      else m.set(d.group, [d]);
    }
    return [...m.entries()];
  }, [paramDefs]);

  const setEntry = (flag: string, patch: Partial<ParamEntry>) => {
    setDraft((d) => {
      if (!d) return d;
      return {
        ...d,
        params: d.params.map((p) => (p.flag === flag ? { ...p, ...patch } : p)),
      };
    });
  };

  const applyV100 = () => {
    setDraft((d) => {
      if (!d) return d;
      return {
        ...d,
        params: d.params.map((p) =>
          p.flag in V100_PRESET ? { ...p, value: V100_PRESET[p.flag], enabled: true } : p,
        ),
      };
    });
  };

  return (
    <div>
      {/* Основные поля */}
      <div className="mb-4 grid grid-cols-2 gap-3 md:grid-cols-4">
        <label className="text-sm">
          <span className="mb-1 block text-gray-400">Имя профиля</span>
          <input
            className="input w-full"
            value={draft.name}
            onChange={(e) => setDraft((d) => (d ? { ...d, name: e.target.value } : d))}
          />
          {running && draft.name !== originalName && (
            <span className="mt-1 block text-xs text-yellow-500">
              контейнер запущен — переименуется при сохранении
            </span>
          )}
        </label>
        <label className="col-span-2 text-sm">
          <span className="mb-1 block text-gray-400">Модель (GGUF из папки моделей)</span>
          <div className="flex gap-1">
            <select
              className="input w-full"
              value={draft.model}
              onChange={(e) => setDraft((d) => (d ? { ...d, model: e.target.value } : d))}
            >
              <option value="">— не выбрана —</option>
              {models.map((m) => (
                <option key={m.name} value={m.name}>
                  {m.name} ({fmtSize(m.size)})
                </option>
              ))}
            </select>
            <button className="btn-ghost" title="Обновить список моделей" onClick={() => void onRefreshModels()}>
              ⟳
            </button>
          </div>
        </label>
        <label className="text-sm">
          <span className="mb-1 block text-gray-400">Порт (хост)</span>
          <input
            type="number"
            className="input w-full"
            value={draft.port}
            min={1024}
            max={65535}
            onChange={(e) =>
              setDraft((d) => (d ? { ...d, port: Number(e.target.value) || d.port } : d))
            }
          />
        </label>
      </div>
      {/* Кнопка набора V100 */}
      <div className="mb-3">
        <button className="btn-ghost" onClick={applyV100} title="Заполнить значениями из docker-compose.v100_CUDA12.yaml (V100, CUDA 12)">
          ⚡ Заполнить проверенным набором V100
        </button>
      </div>

      {/* Группы параметров */}
      {groups.map(([group, defs]) => (
        <fieldset key={group} className="mb-4">
          <legend className="mb-1.5 text-xs font-bold uppercase tracking-wider text-indigo-400">
            {group}
          </legend>
          <div className="grid grid-cols-1 gap-1 lg:grid-cols-2">
            {defs.map((def) => {
              const e = draft.params.find((p) => p.flag === def.flag);
              const enabled = e?.enabled ?? false;
              const value = e?.value ?? "";
              return (
                <div
                  key={def.flag}
                  className="flex items-center gap-2 rounded border border-gray-800 bg-[#11151d] px-2 py-1"
                  title={def.help}
                >
                  <input
                    type="checkbox"
                    checked={enabled}
                    onChange={(ev) => setEntry(def.flag, { enabled: ev.target.checked })}
                    title="Включить параметр в команду запуска"
                  />
                  <span
                    className={`w-40 shrink-0 font-mono text-xs ${
                      enabled ? "text-gray-200" : "text-gray-600"
                    }`}
                  >
                    {def.short ? `${def.short}, ` : ""}
                    {def.flag}
                  </span>
                  {def.kind === "bool" ? (
                    <span className="text-xs text-gray-600">(флаг без значения)</span>
                  ) : def.kind === "select" ? (
                    <select
                      className="input w-32"
                      disabled={!enabled}
                      value={value}
                      onChange={(ev) => setEntry(def.flag, { value: ev.target.value })}
                    >
                      {def.options.map((o) => (
                        <option key={o} value={o}>
                          {o}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <input
                      className="input w-32"
                      disabled={!enabled}
                      type={def.kind === "str" ? "text" : "number"}
                      value={value}
                      step={def.kind === "float" ? "any" : undefined}
                      placeholder={def.default ?? ""}
                      onChange={(ev) => setEntry(def.flag, { value: ev.target.value })}
                    />
                  )}
                </div>
              );
            })}
          </div>
        </fieldset>
      ))}

      {/* Сырые флаги */}
      <fieldset className="mb-4">
        <legend className="mb-1.5 text-xs font-bold uppercase tracking-wider text-indigo-400">
          Дополнительные («сырые») флаги
        </legend>
        <textarea
          className="input h-20 w-full font-mono text-xs"
          placeholder={"Один аргумент в строке, например:\n--stop \"</think>\"\n--mlock"}
          value={draft.rawArgs.join("\n")}
          onChange={(e) =>
            setDraft((d) =>
              d
                ? {
                    ...d,
                    rawArgs: e.target.value
                      .split("\n")
                      .map((s) => s.trim())
                      .filter(Boolean),
                  }
                : d,
            )
          }
        />
      </fieldset>

      {/* Сохранение */}
      <div className="flex items-center gap-3">
        <button className="btn-primary" disabled={busy || !draft.model} onClick={() => void onSave()}>
          💾 Сохранить профиль
        </button>
        {!draft.model && (
          <span className="text-xs text-yellow-500">выберите модель, чтобы сохранить</span>
        )}
        {running && (
          <span className="text-xs text-gray-500">
            контейнер запущен — изменения применятся при следующем старте
          </span>
        )}
      </div>
    </div>
  );
}

