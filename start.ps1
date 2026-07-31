# MyNavicat 一鍵啟動腳本 (PowerShell 7+)
$ErrorActionPreference = "Continue"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "     MyNavicat 正在啟動後端與前端服務..." -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$rootDir = $PSScriptRoot

# 檢查前端 dependencies
if (-not (Test-Path "$rootDir\frontend\node_modules")) {
    Write-Host "[0/3] 檢測到前端 node_modules 不存在，正在執行 npm install..." -ForegroundColor Yellow
    Start-Process powershell -Wait -ArgumentList "-Command", "Set-Location '$rootDir\frontend'; npm install"
}

# 1. 在新視窗啟動後端 Web API (Port 5050)
Write-Host "[1/3] 正在啟動後端 Web API (Port 5050)..." -ForegroundColor Green
$backendCmd = "Set-Location '$rootDir\backend\MyNavicat.Api'; Write-Host '【MyNavicat 後端 API 服務 (Port 5050)】啟動中...' -ForegroundColor Green; dotnet run; Read-Host '按 Enter 鍵關閉視窗...'"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $backendCmd

# 2. 在新視窗啟動前端 Vue 3 Dev Server (Port 5173)
Write-Host "[2/3] 正在啟動前端 Vite Dev Server (Port 5173)..." -ForegroundColor Green
$frontendCmd = "Set-Location '$rootDir\frontend'; Write-Host '【MyNavicat 前端 UI 服務 (Port 5173)】啟動中...' -ForegroundColor Green; npm run dev; Read-Host '按 Enter 鍵關閉視窗...'"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $frontendCmd

# 3. 輪詢檢測埠口是否已成功開啟
Write-Host "[3/3] 等待服務就緒並開啟瀏覽器..." -ForegroundColor Green
$maxRetries = 25
$backendReady = $false
$frontendReady = $false

for ($i = 1; $i -le $maxRetries; $i++) {
    Start-Sleep -Seconds 1

    if (-not $backendReady) {
        $backendCheck = System.Net.Sockets.TcpClient::new()
        try {
            $async = $backendCheck.BeginConnect("127.0.0.1", 5050, $null, $null)
            if ($async.AsyncWaitHandle.WaitOne(300)) {
                $backendCheck.EndConnect($async)
                $backendReady = $true
                Write-Host "  -> 後端服務已就緒 (Port 5050)" -ForegroundColor DarkGreen
            }
        } catch {} finally { $backendCheck.Close() }
    }

    if (-not $frontendReady) {
        $frontendCheck = System.Net.Sockets.TcpClient::new()
        try {
            $async = $frontendCheck.BeginConnect("127.0.0.1", 5173, $null, $null)
            if ($async.AsyncWaitHandle.WaitOne(300)) {
                $frontendCheck.EndConnect($async)
                $frontendReady = $true
                Write-Host "  -> 前端 UI 已就緒 (Port 5173)" -ForegroundColor DarkGreen
            }
        } catch {} finally { $frontendCheck.Close() }
    }

    if ($backendReady -and $frontendReady) {
        break
    }
}

# 打開瀏覽器 (前端 5173 頁面)
Start-Process "http://localhost:5173"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   MyNavicat 啟動完成！已開啟預設瀏覽器" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
