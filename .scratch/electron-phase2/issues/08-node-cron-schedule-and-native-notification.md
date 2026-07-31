# 08 — node-cron 排程與 Windows 原生通知

**What to build:** (1) 在 Electron Main Process 中使用 `node-cron` 實作排程備份引擎，替代原先 Hangfire。(2) 排程任務執行完畢後自動比對 `retainCount`，刪除舊的備份檔案與歷史紀錄。(3) 使用 Electron 原生 `Notification` API：當定時備份成功或失敗時，直接發送 Windows 系統通知卡片。 (4) 前端 `ScheduleView.vue` 串接 CronEditor 與排程 CRUD/啟用/立即執行操作。

**Blocked by:** 07 — CLI 備份還原與 GZIP/ZIP 壓縮/分片 (Electron)

**Status:** ready-for-agent

- [ ] 應用程式啟動或視窗隱藏至 Tray 時，`node-cron` 背景排程依然正常運作
- [ ] 設定 Cron 排程到達指定時間會自動觸發資料庫備份
- [ ] 備份完成或失敗時，Windows 系統右下角彈出原生的 Notification 通知卡片
- [ ] 超過保留份數 (Retain Count) 的舊備份檔案與記錄會被自動清理
- [ ] ScheduleView 可新增、編輯、啟用/停用排程與立即執行一次
