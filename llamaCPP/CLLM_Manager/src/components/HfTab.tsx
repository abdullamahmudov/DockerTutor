import { useEffect, useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { listen, type UnlistenFn } from "@tauri-apps/api/event";
import type { AppConfig, HfFile, HfProgress } from "../types";

interface Props {
  config: AppConfig;
  onDone: () => Promise<void>;
}

function fmtSize(bytes: number): string {
  return `${(bytes / 1024 ** 3).toFixed(1)} ГБ`;
}

export default function HfTab({ config, onDone }: Props) {
  const [repo, setRepo] = useState("");
  const [files, setFiles] = useState<HfFile[]>([]);
  const [selectedFile, setSelectedFile] = useState<string | null>(null);
  const [listing, setListing] = useState(false);
  const [downloading, setDownloading] = useState(false);
  const [progress, setProgress] = useState<HfProgress | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let un: UnlistenFn | undefined;
    listen<HfProgress>("hf-progress", (ev) => {
      setProgress(ev.payload);
      if (ev.payload.done) {
        setDownloading(false);
        if (ev.payload.error) setError(ev.payload.error);
        else void onDone();
      }
    }).then((u) => (un = u));
    return () => un?.();
  }, [onDone]);

  const listFiles = async () => {
    setError(null);
    setListing(true);
    setFiles([]);
    setSelectedFile(null);
    try {
      const res = await invoke<HfFile[]>("hf_list_files", { repo: repo.trim() });
      setFiles(res);
      if (res.length === 0) setError("В репозитории не найдено .gguf файлов");
    } catch (e) {
      setError(String(e));
    } finally {
      setListing(false);
    }
  };

  const download = async () => {
    if (!selectedFile) return;
    setError(null);
    setProgress({ received: 0, total: 0, speed: 0, done: false, error: null });
    setDownloading(true);
    try {
      await invoke("hf_download", { repo: repo.trim(), file: selectedFile });
    } catch (e) {
      setError(String(e));
      setDownloading(false);
    }
  };

  const cancel = () => {
    invoke("hf_cancel").catch(() => {});
  };

  const pct =
    progress && progress.total > 0
      ? Math.min(100, (progress.received / progress.total) * 100)
      : 0;

  return (
    <div className="flex h-full flex-col gap-3 overflow-y-auto">
      {/* Репозиторий */}
      <div className="panel p-4">
        <h2 className="mb-2 text-sm font-bold text-gray-300">
          1. Репозиторий HuggingFace
        </h2>
        <div className="flex gap-2">
          <input
            className="input w-96"
            placeholder="например: Qwen/Qwen3-32B-GGUF"
            value={repo}
            onChange={(e) => setRepo(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && !listing && void listFiles()}
          />
          <button className="btn-primary" disabled={listing || !repo.trim()} onClick={() => void listFiles()}>
            {listing ? "Поиск…" : "Список .gguf файлов"}
          </button>
        </div>
        {config.hfToken ? (
          <div className="mt-2 text-xs text-gray-500">
            HF-токен: задан (gated-репозитории доступны)
          </div>
        ) : (
          <div className="mt-2 text-xs text-gray-600">
            HF-токен не задан — доступны только публичные репозитории (токен можно указать во вкладке «Настройки»)
          </div>
        )}
      </div>

      {/* Файлы */}
      {files.length > 0 && (
        <div className="panel flex min-h-0 flex-1 flex-col p-4">
          <h2 className="mb-2 text-sm font-bold text-gray-300">
            2. Выбор файла ({files.length})
          </h2>
          <div className="min-h-0 flex-1 overflow-y-auto rounded border border-gray-800">
            {files.map((f) => (
              <label
                key={f.path}
                className="flex cursor-pointer items-center gap-3 border-b border-gray-800/60 px-3 py-2 text-sm hover:bg-gray-800/40"
              >
                <input
                  type="radio"
                  name="hf-file"
                  checked={selectedFile === f.path}
                  onChange={() => setSelectedFile(f.path)}
                />
                <span className="flex-1 truncate font-mono text-xs text-gray-300">
                  {f.path}
                </span>
                <span className="text-xs text-gray-500">{fmtSize(f.size)}</span>
              </label>
            ))}
          </div>
        </div>
      )}

      {/* Загрузка */}
      <div className="panel p-4">
        <h2 className="mb-2 text-sm font-bold text-gray-300">3. Скачивание</h2>
        <div className="flex items-center gap-3">
          <button
            className="btn-primary"
            disabled={downloading || !selectedFile}
            onClick={() => void download()}
          >
            {downloading ? "Загрузка…" : "⬇ Скачать в папку моделей"}
          </button>
          {downloading && (
            <button className="btn-danger" onClick={cancel}>
              Отменить
            </button>
          )}
        </div>
        {progress && (
          <div className="mt-3">
            <div className="h-3 w-full overflow-hidden rounded bg-gray-800">
              <div
                className="h-full bg-indigo-500 transition-all"
                style={{ width: `${pct}%` }}
              />
            </div>
            <div className="mt-1 text-xs text-gray-400">
              {progress.total > 0
                ? `${(progress.received / 1024 ** 3).toFixed(2)} / ${(progress.total / 1024 ** 3).toFixed(2)} ГБ (${pct.toFixed(1)}%)`
                : "определение размера…"}
              {progress.speed > 0 &&
                ` — ${(progress.speed / 1024 ** 2).toFixed(1)} МБ/с`}
            </div>
          </div>
        )}
      </div>

      {error && (
        <div className="rounded-md border border-red-800 bg-red-950/60 px-3 py-2 text-sm text-red-300">
          {error}
        </div>
      )}
    </div>
  );
}
