# MyNavicat 桌面單機版一鍵啟動腳本 (PowerShell)
$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     MyNavicat 桌面單機版啟動中..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$rootDir = $PSScriptRoot

# 1. 檢查並安裝套件
if (-not (Test-Path "$rootDir\node_modules")) {
    Write-Host "[1/2] 正在初始化 Node.js 模組..." -ForegroundColor Yellow
    Set-Location '$rootDir'
    npm install
}

# 2. 啟動 Electron 桌面應用程式
Write-Host "[2/2] 啟動 Electron 深色無邊框視窗..." -ForegroundColor Green
Set-Location '$rootDir'
npm run electron:dev
