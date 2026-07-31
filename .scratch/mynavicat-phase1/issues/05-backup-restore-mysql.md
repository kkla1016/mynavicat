# 05 — 備份與還原 — MySQL

**What to build:** 以 MySQL 為目標的完整備份與還原流程。備份：使用者選擇連線 → 資料庫 → 可選資料表 → 壓縮選項（不壓縮 / gz / zip）→ 點擊備份 → 系統呼叫 mysqldump 產生 .sql → 若需壓縮則壓縮 → 記錄 BackupHistory → UI 顯示進度指示與結果。還原：從備份歷史選擇一筆記錄或上傳外部 .sql/.gz/.zip 檔 → 選擇目標連線和資料庫 → 若壓縮則自動解壓 → 呼叫 mysql 還原 → 顯示結果。備份歷史頁面：列表顯示（狀態 badge、時間、大小、耗時）、篩選（連線/資料庫/狀態/時間範圍）、下載、刪除（含檔案）。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] `POST /api/backup` 呼叫 mysqldump 產生備份檔
- [ ] 壓縮選項正常運作（gz / zip）
- [ ] BackupHistory 記錄正確寫入（狀態、檔案路徑、大小、耗時）
- [ ] 備份失敗時 BackupHistory 記錄錯誤訊息
- [ ] `POST /api/backup/restore` 可從歷史記錄還原
- [ ] `POST /api/backup/restore` 可接受上傳的備份檔
- [ ] 壓縮的備份檔自動解壓後還原
- [ ] `GET /api/backup/history` 支援分頁和篩選
- [ ] `GET /api/backup/history/{id}/download` 可下載備份檔
- [ ] `DELETE /api/backup/history/{id}` 刪除記錄和檔案
- [ ] BackupService 單元測試通過
- [ ] 前端備份頁面可選擇連線/資料庫/表/壓縮後備份
- [ ] 前端還原頁面可從歷史選擇或上傳檔案後還原
- [ ] 前端備份歷史頁面可列表/篩選/下載/刪除
- [ ] 進度指示在備份/還原執行中正確顯示
