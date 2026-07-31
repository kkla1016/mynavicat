# 02 — 深色標題列與 System Tray 常駐

**What to build:** (1) 前端 `CustomTitlebar.vue` 元件：客製化深色標題列，包含應用標誌、拖曳區域、視窗最小化、最大化/還原與關閉按鈕。(2) Electron Main Process 系統工具列 (System Tray) 常駐：建立右下角 Tray 圖示與選單（顯示主視窗、關於、完全退出）。使用者點擊視窗關閉 (X) 時改為隱藏至 System Tray，保持背景排程持續運轉。

**Blocked by:** 01 — Electron 桌面骨架與 Preload 橋接

**Status:** ready-for-agent

- [ ] `CustomTitlebar.vue` 正確顯示並支援視窗拖曳與視窗控制按鈕
- [ ] 點擊標題列最小化、最大化按鈕可正確改變 Electron 視窗狀態
- [ ] 啟動後右下角系統列出現 MyNavicat Tray 圖示
- [ ] 點擊視窗關閉 (X) 鈕，視窗隱藏至 System Tray 而不安閉程式
- [ ] 雙擊 Tray 圖示或選擇選單「顯示主視窗」可恢復主視窗
- [ ] 點擊 Tray 右鍵選單「完全退出」可正常結束 Electron 處理程序
