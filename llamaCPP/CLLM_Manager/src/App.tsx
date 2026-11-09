import { useCallback, useEffect, useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { listen, type UnlistenFn } from "@tauri-apps/api/event";
import type {
  AppConfig,
  ContainerStatus,
  ModelInfo,
  ParamDef,
  Profile,
} from "./types";
import ProfilesTab from "./components/ProfilesTab";
import HfTab from "./components/HfTab";
import SettingsTab from "./components/SettingsTab";

type Tab = "profiles" | "hf" | "settings";

export default function App() {
  const [tab, setTab] = useState<Tab>("profiles");
  const [config, setConfig] = useState<AppConfig | null>(null);
  const [paramDefs, setParamDefs] = useState<ParamDef[]>([]);
  const [models, setModels] = useState<ModelInfo[]>([]);
  const [profiles, setProfiles] = useState<Profile[]>([]);
  const [statuses, setStatuses] = useState<ContainerStatus[]>([]);
  const [selected, setSelected] = useState<string | null>(null);
  const [viewLogs, setViewLogs] = useState<string[]>([]);
  const [dockerOk, setDockerOk] = useState<boolean | null>(null);
  const [error, setError] = useState<string | null>(null);

  const refreshModels = useCallback(async () => {
    try {
      setModels(await invoke<ModelInfo[]>("list_models"));
    } catch (e) {
      setError(String(e));
    }
  }, []);

  const refreshStatuses = useCallback(async () => {
    try {
      setStatuses(await invoke<ContainerStatus[]>("list_containers"));
    } catch {
      /* docker недоступен — статусы не критичны */
    }
  }, []);

  // начальная загрузка
  useEffect(() => {
    (async () => {
      try {
        const [cfg, defs, profs] = await Promise.all([
          invoke<AppConfig>("get_config"),
          invoke<ParamDef[]>("get_param_defs"),
          invoke<Profile[]>("get_profiles"),
        ]);
        setConfig(cfg);
        setParamDefs(defs);
        setProfiles(profs);
        setSelected(profs.length > 0 ? profs[0].name : null);
        await Promise.all([refreshModels(), refreshStatuses()]);
      } catch (e) {
        setError(String(e));
      }
    })();
  }, [refreshModels, refreshStatuses]);

  // опрос статусов контейнеров + docker
  useEffect(() => {
    const tick = async () => {
      await refreshStatuses();
      try {
        setDockerOk(await invoke<boolean>("docker_available"));
      } catch {
        setDockerOk(false);
      }
    };
    tick();
    const t = setInterval(tick, 5000);
    return () => clearInterval(t);
  }, [refreshStatuses]);

  // live-логи выбранного контейнера
  useEffect(() => {
    if (!selected) return;
    let un: UnlistenFn | undefined;
    setViewLogs([]);
    invoke("open_logs", { name: selected }).catch(() => {});
    listen<{ line: string }>("container-log", (ev) => {
      setViewLogs((prev) => {
        const next = [...prev, ev.payload.line];
        if (next.length > 3000) next.splice(0, next.length - 3000);
        return next;
      });
    }).then((u) => (un = u));
    return () => {
      un?.();
    };
  }, [selected]);

  const saveProfilesToDisk = useCallback(async (list: Profile[]) => {
    await invoke("save_profiles", { profiles: list });
  }, []);

  const handleNewProfile = useCallback(async () => {
    if (!config) return;
    const def = await invoke<Profile>("new_profile", {
      portBase: config.portBase,
      count: profiles.length,
    });
    const next = [...profiles, def];
    setProfiles(next);
    setSelected(def.name);
    await saveProfilesToDisk(next);
  }, [config, profiles, saveProfilesToDisk]);

  const handleSaveProfile = useCallback(
    async (p: Profile) => {
      const next = profiles.map((x) => (x.name === p.name ? p : x));
      setProfiles(next);
      await saveProfilesToDisk(next);
    },
    [profiles, saveProfilesToDisk],
  );

  const handleDeleteProfile = useCallback(
    async (name: string) => {
      const st = statuses.find((s) => s.profile === name);
      if (st?.running) {
        setError("Сначала остановите контейнер");
        return;
      }
      const next = profiles.filter((x) => x.name !== name);
      setProfiles(next);
      if (selected === name) setSelected(next.length > 0 ? next[0].name : null);
      await saveProfilesToDisk(next);
    },
    [profiles, statuses, selected, saveProfilesToDisk],
  );

  const handleStart = useCallback(
    async (name: string) => {
      setError(null);
      try {
        await invoke("start_container", { name });
        await refreshStatuses();
      } catch (e) {
        setError(String(e));
      }
    },
    [refreshStatuses],
  );

  const handleStop = useCallback(
    async (name: string) => {
      setError(null);
      try {
        await invoke("stop_container", { name });
        await refreshStatuses();
      } catch (e) {
        setError(String(e));
      }
    },
    [refreshStatuses],
  );

  const handleConfigSaved = useCallback((cfg: AppConfig) => {
    setConfig(cfg);
  }, []);

  if (!config) {
    return (
      <div className="flex h-full items-center justify-center text-gray-400">
        Загрузка…
      </div>
    );
  }

  const selectedStatus = statuses.find((s) => s.profile === selected);
  return (
    <div className="flex h-full flex-col gap-2 p-3">
      {/* Шапка */}
      <header className="flex items-center gap-4">
        <h1 className="text-lg font-bold text-indigo-400">CLLM Manager</h1>
        <nav className="flex gap-1">
          {(
            [
              ["profiles", "Профили"],
              ["hf", "Загрузка моделей (HF)"],
              ["settings", "Настройки"],
            ] as [Tab, string][]
          ).map(([t, label]) => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`btn ${tab === t ? "bg-indigo-600 text-white" : "border border-gray-700 text-gray-300 hover:bg-gray-800"}`}
            >
              {label}
            </button>
          ))}
        </nav>
        <div className="ml-auto flex items-center gap-2 text-sm">
          <span
            className={`inline-block h-2.5 w-2.5 rounded-full ${
              dockerOk ? "bg-green-500" : dockerOk === false ? "bg-red-500" : "bg-gray-600"
            }`}
          />
          <span className="text-gray-400">
            {dockerOk === true
              ? "Docker доступен"
              : dockerOk === false
                ? "Docker недоступен"
                : "Проверка Docker…"}
          </span>
        </div>
      </header>

      {error && (
        <div className="rounded-md border border-red-800 bg-red-950/60 px-3 py-2 text-sm text-red-300">
          {error}
          <button className="ml-3 text-red-400 underline" onClick={() => setError(null)}>
            закрыть
          </button>
        </div>
      )}

      <main className="min-h-0 flex-1">
        {tab === "profiles" && (
          <ProfilesTab
            config={config}
            paramDefs={paramDefs}
            models={models}
            profiles={profiles}
            statuses={statuses}
            selected={selected}
            selectedStatus={selectedStatus}
            viewLogs={viewLogs}
            onSelect={setSelected}
            onNew={handleNewProfile}
            onDelete={handleDeleteProfile}
            onSave={handleSaveProfile}
            onStart={handleStart}
            onStop={handleStop}
            onRefreshModels={refreshModels}
          />
        )}
        {tab === "hf" && <HfTab config={config} onDone={refreshModels} />}
        {tab === "settings" && (
          <SettingsTab config={config} onSaved={handleConfigSaved} />
        )}
      </main>
    </div>
  );
}

