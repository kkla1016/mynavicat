# 09 — SQL Server 全棧支援

**What to build:** 實作 SqlServerProvider，讓 SQL Server 連線在系統中可完整使用。Provider 需實作：連線測試（使用 Microsoft.Data.SqlClient）、取得資料庫清單、取得資料表清單/結構/資料、產生 sqlcmd/bcp 備份命令、產生 sqlcmd 還原命令。SQL Server 的備份方式與 MySQL/PostgreSQL 不同（使用 BACKUP DATABASE T-SQL 或 sqlcmd + bcp），Provider 需正確處理。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] SqlServerProvider 實作 IDbProvider 介面的所有方法
- [ ] SqlServerProvider 連線字串正確使用 SqlClient 格式
- [ ] SqlServerProvider 備份命令正確產生（sqlcmd + BACKUP DATABASE 或 bcp）
- [ ] SqlServerProvider 還原命令正確產生（sqlcmd + RESTORE DATABASE）
- [ ] SqlServerProvider 單元測試通過
- [ ] 前端可建立 SQL Server 連線並測試成功
- [ ] SQL Server 連線可正常瀏覽資料庫/資料表
- [ ] SQL Server 備份/還原流程正常運作
