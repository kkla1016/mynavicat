# MyNavicat 一鍵啟動腳本 (PowerShell)
$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     MyNavicat 正在啟動後端與前端服務..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$rootDir = $PSScriptRoot

# 1. 在新視窗啟動後端 Web API
Write-Host "[1/3] 正在啟動後端 ASP.NET Core Web API (Port 5000)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$rootDir\backend\MyNavicat.Api'; Write-Host '後端 API 服務啟動中...' -ForegroundColor Yellow; dotnet run"

# 2. 在新視窗啟動前端 Vue 3 Dev Server
Write-Host "[2/3] 正在啟動前端 Vite Dev Server (Port 5173)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$rootDir\frontend'; Write-Host '前端 Dev Server 啟動中...' -ForegroundColor Yellow; npm run dev"

# 3. 等待 3 秒後自動開啟瀏覽器
Start-Sleep -Seconds 3
Write-Host "[3/3] 正在自動開啟預設瀏覽器存取 http://localhost:5173 ..." -ForegroundColor Green
Start-Process "http://localhost:5173"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   MyNavicat 服務啟動完成！" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
