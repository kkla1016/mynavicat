# 08 — PostgreSQL 全棧支援

**What to build:** 實作 PostgreSqlProvider，讓 PostgreSQL 連線在系統中可完整使用。Provider 需實作：連線測試（使用 Npgsql）、取得資料庫清單、取得資料表清單/結構/資料、產生 pg_dump 備份命令、產生 psql 還原命令。完成後，使用者可在連線管理中選擇 PostgreSQL 類型建立連線，並使用瀏覽、備份/還原、排程備份、匯出入等所有已建立的功能——因為 UI 和服務層邏輯已在 Ticket 03~07 中建好，只需 Provider 正確回應即可。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] PostgreSqlProvider 實作 IDbProvider 介面的所有方法
- [ ] PostgreSqlProvider 連線字串正確使用 Npgsql 格式
- [ ] PostgreSqlProvider 備份命令正確呼叫 pg_dump
- [ ] PostgreSqlProvider 還原命令正確呼叫 psql
- [ ] PostgreSqlProvider 單元測試通過
- [ ] 前端可建立 PostgreSQL 連線並測試成功
- [ ] PostgreSQL 連線可正常瀏覽資料庫/資料表
- [ ] PostgreSQL 備份/還原流程正常運作
