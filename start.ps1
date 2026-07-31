# MyNavicat 桌面單機版一鍵啟動腳本 (PowerShell)
$ErrorActionPreference = "Continue"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     MyNavicat 桌面單機版啟動中..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$rootDir = $PSScriptRoot
Set-Location '$rootDir'

# 1. 檢查並安裝 node_modules
if (-not (Test-Path "$rootDir\node_modules")) {
    Write-Host "[1/3] 正在初始化 Node.js 模組 (npm install)..." -ForegroundColor Yellow
    npm install
}

# 2. 構建 Electron 與前端包
Write-Host "[2/3] 構建 Electron 主進程與前端靜態資源..." -ForegroundColor Yellow
npm run build:electron
npm run build --prefix frontend

# 3. 啟動 Electron
Write-Host "[3/3] 啟動 Electron 桌面應用程式..." -ForegroundColor Green
npx electron .

Read-Host "按 Enter 鍵關閉視窗..."
