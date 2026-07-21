# Stop all IIQF Project Services
# This script terminates the .NET WebAPI, Python ML process, and Frontend Node runner simultaneously.

Write-Host "🛑 Stopping all IIQF Project services..." -ForegroundColor Red
Write-Host "----------------------------------------" -ForegroundColor Red

# Kill the target running processes
Stop-Process -Name "BhDream.WebAPI" -ErrorAction SilentlyContinue
Stop-Process -Name "python" -ErrorAction SilentlyContinue
Stop-Process -Name "node" -ErrorAction SilentlyContinue
Stop-Process -Name "GreeksEngine" -ErrorAction SilentlyContinue

Write-Host "----------------------------------------" -ForegroundColor Green
Write-Host "✅ Stopped Backend (.NET), Worker (Python), Greeks Engine (C++), and UI (Node/React) successfully!" -ForegroundColor Green
Read-Host "Press Enter to exit..."
