# MyNavicat 桌面單機版一鍵啟動腳本 (PowerShell)
$ErrorActionPreference = "Stop"

# 取得腳本所在根目錄
$rootDir = $PSScriptRoot
if (-not $rootDir) {
    $rootDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
}

Set-Location "$rootDir"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     MyNavicat 桌面單機版啟動中..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 啟動 Electron 應用程式
npx electron .
