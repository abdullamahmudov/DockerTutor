import { useEffect, useRef, useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { listen, type UnlistenFn } from "@tauri-apps/api/event";
import type { AppConfig } from "../types";

interface Props {
  config: AppConfig;
  onSaved: (cfg: AppConfig) => void;
}

export default function SettingsTab({ config, onSaved }: Props) {
  const [draft, setDraft] = useState<AppConfig>(config);
  const [saved, setSaved] = useState(false);
  const [buildLog, setBuildLog] = useState<string[]>([]);
  const [building, setBuilding] = useState(false);
  const logRef = useRef<HTMLDivElement>(null);

  useEffect(() => setDraft(config), [config]);

  useEffect(() => {
    let un: UnlistenFn | undefined;
    listen<{ line: string }>("build-log", (ev) => {
      setBuildLog((prev) => {
        const next = [...prev, ev.payload.line];
        if (next.length > 1500) next.splice(0, next.length - 1500);
        return next;
      });
    }).then((u) => (un = u));
    return () => un?.();
  }, []);

  useEffect(() => {
    if (logRef.current) logRef.current.scrollTop = logRef.current.scrollHeight;
  }, [buildLog]);

  const set = (patch: Partial<AppConfig>) => setDraft((d) => ({ ...d, ...patch }));

  const setBuildArg = (key: keyof AppConfig["buildArgs"], value: string) =>
    setDraft((d) => ({ ...d, buildArgs: { ...d.buildArgs, [key]: value } }));

  const save = async () => {
    await invoke("save_config", { cfg: draft });
    onSaved(draft);
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  const build = async () => {
    setBuilding(true);
    setBuildLog([]);
    try {
      await invoke("build_image", { cfg: draft });
    } catch (e) {
      setBuildLog((prev) => [...prev, `ОШИБКА: ${e}`]);
    } finally {
      setBuilding(false);
    }
  };

  return (
    <div className="flex h-full flex-col gap-3 overflow-y-auto pr-1">
      <div className="panel p-4">
        <h2 className="mb-3 text-sm font-bold text-gray-300">
          Конфигурация приложения (config.yaml)
        </h2>
        <div className="grid grid-cols-2 gap-3">
          <label className="col-span-2 text-sm">
            <span className="mb-1 block text-gray-400">Папка с моделями (GGUF)</span>
            <input
              className="input w-full"
              value={draft.modelsPath}
              onChange={(e) => set({ modelsPath: e.target.value })}
            />
          </label>
          <label className="col-span-2 text-sm">
            <span className="mb-1 block text-gray-400">Dockerfile для сборки образа</span>
            <input
              className="input w-full"
              value={draft.dockerfile}
              onChange={(e) => set({ dockerfile: e.target.value })}
            />
          </label>
          <label className="text-sm">
            <span className="mb-1 block text-gray-400">Docker-образ (по умолчанию)</span>
            <input
              className="input w-full"
              value={draft.image}
              onChange={(e) => set({ image: e.target.value })}
            />
          </label>
          <label className="text-sm">
            <span className="mb-1 block text-gray-400">
              Базовый порт (автоаллокация новых профилей)
            </span>
            <input
              type="number"
              className="input w-full"
              value={draft.portBase}
              onChange={(e) => set({ portBase: Number(e.target.value) || 9722 })}
            />
          </label>
          <label className="col-span-2 text-sm">
            <span className="mb-1 block text-gray-400">
              HuggingFace токен (опционально, для gated-моделей)
            </span>
            <input
              type="password"
              className="input w-full"
              value={draft.hfToken ?? ""}
              placeholder="hf_…"
              onChange={(e) => set({ hfToken: e.target.value || null })}
            />
          </label>
        </div>
        <h3 className="mb-2 mt-5 text-xs font-bold uppercase tracking-wider text-indigo-400">
          Аргументы сборки образа (V100.Dockerfile)
        </h3>
        <div className="grid grid-cols-4 gap-3">
          <label className="text-sm">
            <span className="mb-1 block text-gray-400">CUDA_VERSION</span>
            <input
              className="input w-full"
              value={draft.buildArgs.cudaVersion}
              onChange={(e) => setBuildArg("cudaVersion", e.target.value)}
            />
          </label>
          <label className="text-sm">
            <span className="mb-1 block text-gray-400">UBUNTU_VERSION</span>
            <input
              className="input w-full"
              value={draft.buildArgs.ubuntuVersion}
              onChange={(e) => setBuildArg("ubuntuVersion", e.target.value)}
            />
          </label>
          <label className="text-sm">
            <span className="mb-1 block text-gray-400">LLAMA_CPP_REF</span>
            <input
              className="input w-full"
              value={draft.buildArgs.llamaCppRef}
              onChange={(e) => setBuildArg("llamaCppRef", e.target.value)}
            />
          </label>
          <label className="text-sm">
            <span className="mb-1 block text-gray-400">CUDA_ARCH</span>
            <input
              className="input w-full"
              value={draft.buildArgs.cudaArch}
              onChange={(e) => setBuildArg("cudaArch", e.target.value)}
            />
          </label>
        </div>

        <div className="mt-4 flex items-center gap-3">
          <button className="btn-primary" onClick={() => void save()}>
            💾 Сохранить конфигурацию
          </button>
          <button className="btn-ghost" disabled={building} onClick={() => void build()}>
            {building ? "Сборка…" : "🔨 Собрать образ (docker build)"}
          </button>
          {saved && <span className="text-xs text-green-400">сохранено</span>}
        </div>
      </div>

      {buildLog.length > 0 && (
        <div className="panel flex h-72 shrink-0 flex-col">
          <div className="border-b border-gray-800 px-3 py-1.5 text-sm text-gray-400">
            Лог сборки образа
          </div>
          <div ref={logRef} className="min-h-0 flex-1 overflow-y-auto p-2">
            {buildLog.map((l, i) => (
              <div key={i} className="log-line">
                {l}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
