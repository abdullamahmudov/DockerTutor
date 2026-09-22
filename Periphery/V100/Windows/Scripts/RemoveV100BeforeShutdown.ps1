# gpu-preshutdown.ps1
# Запускается как скрипт завершения работы (Group Policy: Computer Configuration ->
# Windows Settings -> Scripts (Startup/Shutdown) -> Shutdown), выполняется от имени SYSTEM.
#
# Цель:
#   1. Проверить, есть ли активные процессы на GPU (nvidia-smi --query-compute-apps).
#   2. Если есть — аккуратно остановить известный docker-compose стек с llama.cpp.
#   3. Подождать и перепроверить.
#   4. В любом случае отключить и удалить устройство Tesla из PnP-дерева,
#      чтобы Windows не унесла в следующую сессию "плохой" кэш ресурсов
#      и карта после перезагрузки снова не упала в Код 10.

$ErrorActionPreference = "Continue"
$logFile = "C:\Scripts\gpu-preshutdown.log"

function Log {
    param([string]$msg)
    "$(Get-Date -Format o)  $msg" | Out-File -FilePath $logFile -Append -Encoding utf8
}

Log "=== gpu-preshutdown: старт ==="

# --- ПОДСТАВЬТЕ СВОЙ ПУТЬ ---
# Папка, где лежит docker-compose.yml с llama.cpp
$composeDir = "D:\Work\Custom\DockerTutor\DockerTutor\llamaCPP\docker-compose.cmake_v100.yaml"
# ----------------------------

# 1. Проверяем активные compute-процессы на GPU
$gpuProcs = $null
try {
    $gpuProcs = & nvidia-smi --query-compute-apps=pid,process_name,used_memory --format=csv,noheader 2>$null
} catch {
    Log "nvidia-smi не найден или завершился с ошибкой: $_"
}

if ($gpuProcs) {
    Log "Обнаружены активные процессы на GPU:`n$gpuProcs"

    if (Test-Path $composeDir) {
        Log "Пытаюсь корректно остановить docker compose в $composeDir"
        Push-Location $composeDir
        try {
            $out = docker compose down 2>&1
            Log "docker compose down: $out"
        } catch {
            Log "Ошибка при docker compose down: $_"
        }
        Pop-Location
    } else {
        Log "Путь $composeDir не найден — пропускаю остановку docker compose"
    }

    # Даём время освободить GPU
    Start-Sleep -Seconds 8

    # Перепроверяем
    $gpuProcs = & nvidia-smi --query-compute-apps=pid,process_name,used_memory --format=csv,noheader 2>$null
    if ($gpuProcs) {
        Log "GPU всё ещё занят после попытки остановки: $gpuProcs — продолжаю в любом случае"
    } else {
        Log "GPU освобождён."
    }
} else {
    Log "Активных процессов на GPU не найдено."
}

# 2. Отключаем и удаляем устройство Tesla
try {
    Get-PnpDevice -FriendlyName "*Tesla*" -ErrorAction Stop | Disable-PnpDevice -Confirm:$false -ErrorAction Stop
    Log "Disable-PnpDevice: успешно"
} catch {
    Log "Disable-PnpDevice: ошибка: $_"
}

try {
    $removeOut = pnputil /remove-device "PCI\VEN_10DE&DEV_1DB5&SUBSYS_124910DE&REV_A1\4&8BD6E8D&0&0008" 2>&1
    Log "pnputil /remove-device: $removeOut"
} catch {
    Log "pnputil /remove-device: ошибка: $_"
}

Log "=== gpu-preshutdown: конец ==="