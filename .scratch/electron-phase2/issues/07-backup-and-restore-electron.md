# 07 — CLI 備份還原與 GZIP/ZIP 壓縮/分片 (Electron)

**What to build:** (1) Main Process `child_process.spawn` 呼叫資料庫 CLI 生成備份檔與執行還原命令。(2) 使用 Node.js `zlib` / `archiver` 處理 GZIP (.gz) 與 ZIP (.zip) 壓縮與解壓。(3) 超大型庫分片備份 (Chunked Backup)：備份檔大於 Chunk 閥值時自動切割為 `.part1`, `.part2` 等檔案並寫入 SQLite 歷史紀錄；還原時自動串接 Parts 與解壓。(4) 前端 BackupView, RestoreView, HistoryView 畫面串接。

**Blocked by:** 05 — 桌面連線管理與 MySqlProvider (Electron)

**Status:** ready-for-agent

- [ ] 執行即時備份能產生正確的 `.sql` 備份檔
- [ ] 勾選壓縮後能自動產生 `.sql.gz` 或 `.sql.zip` 檔案
- [ ] 超過指定大小的分片備份能自動分割為多個 Part 檔並於 SQLite 紀錄關聯
- [ ] 能從歷史紀錄選擇備份或上傳外部備份檔進行資料庫還原
- [ ] HistoryView 畫面可篩選、下載、刪除備份檔
- [ ] 單元測試驗證分片切割與合併還原邏輯
