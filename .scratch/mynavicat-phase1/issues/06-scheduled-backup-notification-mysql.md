# 06 — 排程備份與桌面通知 — MySQL

**What to build:** 排程備份管理與即時桌面通知。排程：使用者設定 Cron 表達式（透過視覺化 CronEditor 元件）、選擇連線/資料庫/資料表/壓縮選項/保留份數 → 建立排程 → Hangfire 註冊 RecurringJob → 排程到時觸發 BackupService 執行備份 → 更新 LastRunAt/NextRunAt → 若設定了保留份數則自動清理最舊的備份。通知：排程備份完成或失敗後，NotificationService 透過 SignalR Hub 推送事件到前端 → 前端的 useSignalR 收到事件 → useDesktopNotification 呼叫 Browser Notification API 推送桌面通知。排程管理頁面：CRUD、啟用/停用 toggle、立即執行、下次預計時間顯示。

**Blocked by:** 05 — 備份與還原 — MySQL

**Status:** ready-for-agent

- [ ] `POST /api/schedules` 建立排程並在 Hangfire 註冊 RecurringJob
- [ ] 排程到時正確觸發備份流程
- [ ] `POST /api/schedules/{id}/toggle` 啟用/停用排程
- [ ] `POST /api/schedules/{id}/run-now` 立即觸發一次
- [ ] 保留份數自動清理最舊的備份記錄和檔案
- [ ] ScheduleService 單元測試通過
- [ ] NotificationService 在備份完成/失敗後呼叫 SignalR Hub
- [ ] NotificationService 單元測試通過
- [ ] 前端成功建立 SignalR WebSocket 連線
- [ ] 前端收到 BackupCompleted 事件後彈出桌面通知
- [ ] 前端收到 BackupFailed 事件後彈出桌面通知含錯誤摘要
- [ ] 前端首次使用時請求 Notification API 授權
- [ ] CronEditor 元件可視覺化編輯 Cron 表達式
- [ ] 排程管理頁面可 CRUD / toggle / run-now
- [ ] 排程清單顯示下次預計執行時間
