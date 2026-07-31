# 11 — SQLite 全棧支援

**What to build:** 實作 SqliteProvider，讓 SQLite 連線在系統中可完整使用。SQLite 為本機檔案型資料庫，連線方式與其他資料庫不同——不需要 IP/Port/帳號密碼，只需指定檔案路徑。前端連線表單需根據資料庫類型動態調整（選擇 SQLite 時隱藏 IP/Port/帳號密碼欄位，改為顯示檔案路徑輸入）。備份使用 sqlite3 CLI 的 `.backup` 命令或直接複製檔案。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] SqliteProvider 實作 IDbProvider 介面的所有方法
- [ ] SqliteProvider 連線字串正確使用 Microsoft.Data.Sqlite 格式
- [ ] SqliteProvider 備份命令使用 sqlite3 CLI `.backup` 或檔案複製
- [ ] SqliteProvider 單元測試通過
- [ ] 前端連線表單選擇 SQLite 時動態切換為檔案路徑輸入
- [ ] SQLite 連線可正常使用所有功能
