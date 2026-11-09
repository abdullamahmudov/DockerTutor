# CLLM Manager — запуск dev-сессии отдельным процессом
# Использование: powershell -ExecutionPolicy Bypass -File run-dev.ps1
$ErrorActionPreference = 'Continue'
$env:Path = "$env:USERPROFILE\.cargo\bin;" + $env:Path
Set-Location 'd:\Work\Custom\DockerTutor\DockerTutor\llamaCPP\CLLM_Manager'
npm run tauri dev 2>&1 | Out-File -FilePath 'd:\Work\Custom\DockerTutor\DockerTutor\llamaCPP\CLLM_Manager\tauri-dev.log' -Encoding utf8
