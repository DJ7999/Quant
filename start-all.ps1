# IIQF Project Services Orchestrator
# This script launches the Backend WebAPI, Python ML Worker, and Frontend UI in separate terminal windows.

Write-Host "🚀 IIQF Project Services Orchestrator 🚀" -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Cyan

# Locate project paths
$ProjectRoot = Get-Item -Path "."
$DotNetDir = Join-Path $ProjectRoot.FullName "Service\DotNetService\BhDream.WebAPI"
$PythonDir = Join-Path $ProjectRoot.FullName "Ml\ml_trainer"
$FrontendDir = Join-Path $ProjectRoot.FullName "Ui\quant"
$CppDir = Join-Path $ProjectRoot.FullName "Service\CppGreeksEngine\GreeksEngine\x64\Debug"

# 1. Start .NET Backend
Write-Host "1. Starting Backend WebAPI (.NET)..." -ForegroundColor Yellow
Start-Process powershell -WorkingDirectory $DotNetDir -ArgumentList "-NoExit", "-Command", "dotnet run --launch-profile https"

# 2. Start Python ML Worker
Write-Host "2. Starting ML Trainer & Worker (Python)..." -ForegroundColor Yellow
Start-Process powershell -WorkingDirectory (Join-Path $PythonDir "app") -ArgumentList "-NoExit", "-Command", "..\.venv\Scripts\python -m uvicorn main:app --host 127.0.0.1 --port 8000"

# 3. Start React Frontend
Write-Host "3. Starting Frontend App (Vite/React)..." -ForegroundColor Yellow
Start-Process powershell -WorkingDirectory $FrontendDir -ArgumentList "-NoExit", "-Command", "npm run dev"

# 4. Start C++ Greeks Engine
Write-Host "4. Starting Greeks Engine (C++)..." -ForegroundColor Yellow
Start-Process powershell -WorkingDirectory $CppDir -ArgumentList "-NoExit", "-Command", ".\GreeksEngine.exe"

Write-Host "----------------------------------------" -ForegroundColor Green
Write-Host "✅ All 4 services launched in separate windows!" -ForegroundColor Green
Write-Host "You can close this orchestrator window." -ForegroundColor Gray
Read-Host "Press Enter to exit..."
