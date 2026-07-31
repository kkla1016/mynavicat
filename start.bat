@echo off
chcp 65001 > nul
title MyNavicat 桌面單機版
echo ==========================================
echo      MyNavicat 桌面單機版啟動中...
echo ==========================================

powershell -ExecutionPolicy Bypass -File "%~dp0start.ps1"
