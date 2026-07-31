# 12 — Oracle 全棧支援

**What to build:** 實作 OracleProvider，讓 Oracle 連線在系統中可完整使用。使用 Oracle.ManagedDataAccess.Core 進行連線和查詢。備份使用 exp/expdp CLI 工具。此 ticket 優先級較低，因為 Oracle 需要額外的 Oracle Instant Client 安裝和授權，但架構上已預留支援。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] OracleProvider 實作 IDbProvider 介面的所有方法
- [ ] OracleProvider 連線字串正確使用 Oracle 格式（TNS / Easy Connect）
- [ ] OracleProvider 備份命令正確呼叫 exp 或 expdp
- [ ] OracleProvider 還原命令正確呼叫 imp 或 impdp
- [ ] OracleProvider 單元測試通過
- [ ] 前端可建立 Oracle 連線並測試成功
- [ ] Oracle 連線可正常使用所有功能
- [ ] 未安裝 Oracle Instant Client 時提供清晰的錯誤提示
