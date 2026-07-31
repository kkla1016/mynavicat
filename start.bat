@echo off
chcp 65001 > nul
title MyNavicat 桌面單機版

cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "%~dp0start.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo 啟動發生錯誤，請按任意鍵退出...
    pause > nul
)
