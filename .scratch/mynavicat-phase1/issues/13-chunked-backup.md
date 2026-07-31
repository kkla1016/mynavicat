# 13 — 分片備份

**What to build:** 大型資料庫的分片備份功能。當使用者對大型資料庫執行備份時，系統自動按資料表分片，每個分片產生獨立的備份檔案。BackupHistory 中以 ParentBackupId 串聯主記錄與分片記錄，ChunkIndex 和 TotalChunks 標記分片順序。前端備份歷史頁面顯示 parent/chunk 的層次關係（可展開/收合）。還原時可選擇整組還原（依序還原所有分片）或單獨還原特定分片。

**Blocked by:** 05 — 備份與還原 — MySQL

**Status:** ready-for-agent

- [ ] BackupService 支援分片備份模式（按資料表分割）
- [ ] 分片備份產生多個獨立檔案，每個檔案對應一或多個資料表
- [ ] BackupHistory 主記錄的 BackupType 設為 "Chunked"
- [ ] 每個分片的 BackupHistory 記錄正確設定 ParentBackupId、ChunkIndex、TotalChunks
- [ ] 分片備份各個分片可獨立壓縮
- [ ] 還原時可選擇整組還原（依序還原所有分片）
- [ ] 還原時可選擇單獨還原特定分片
- [ ] 分片備份的保留份數清理正確處理整組刪除
- [ ] BackupService 分片相關單元測試通過
- [ ] 前端備份歷史頁面以 parent-chunk 層次展示分片備份
