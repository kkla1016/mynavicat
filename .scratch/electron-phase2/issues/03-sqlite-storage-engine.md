# 03 — Better-SQLite3 本地儲存模組

**What to build:** 在 Electron Main Process 中整合 `better-sqlite3`（或 `@vscode/sqlite3`），初始化用戶 `AppData/Roaming/MyNavicat/app.db` 資料庫。建立 `connections`, `backup_schedules`, `backup_histories` 資料表結構。封裝 StorageService，為 IPC Handler 提供高效安全的本地 SQL 資料存取介面。

**Blocked by:** 01 — Electron 桌面骨架與 Preload 橋接

**Status:** ready-for-agent

- [ ] 應用程式啟動時自動於用戶 AppData 目錄建立 `app.db` 資料庫
- [ ] 自動執行資料庫 Migration 初始化三個核心資料表
- [ ] StorageService 提供 connections, schedules, histories 的完整 CRUD 操作
- [ ] 能妥善處理預先存在的舊 SQLite `app.db` 資料庫檔案
- [ ] 單元測試驗證 SQL 查詢語法與 Better-SQLite3 寫入讀取
