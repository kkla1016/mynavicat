# 10 — MariaDB 全棧支援

**What to build:** 實作 MariaDbProvider，讓 MariaDB 連線在系統中可完整使用。MariaDB 與 MySQL 協議相容，因此 MariaDbProvider 可繼承或擴展 MySqlProvider，僅覆寫有差異的部分（如系統表查詢、版本偵測）。使用 mysqldump / mysql CLI 工具進行備份/還原。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] MariaDbProvider 實作完成（繼承/擴展 MySqlProvider）
- [ ] MariaDbProvider 正確識別 MariaDB 特有的系統表或行為差異
- [ ] MariaDbProvider 單元測試通過
- [ ] 前端可建立 MariaDB 連線並測試成功
- [ ] MariaDB 連線可正常使用所有功能
