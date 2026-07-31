@echo off
chcp 65001 > nul
title MyNavicat 一鍵啟動程序
echo ==========================================
echo      MyNavicat 正在啟動後端與前端服務...
echo ==========================================

powershell -ExecutionPolicy Bypass -File "%~dp0start.ps1"
