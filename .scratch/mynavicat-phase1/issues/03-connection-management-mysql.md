# 03 — 連線管理（CRUD + 測試連線）— MySQL

**What to build:** 完整的資料庫連線管理功能，以 MySQL 為 tracer bullet 貫穿全棧。定義 IDbProvider 介面和 DbProviderFactory 工廠模式。實作 MySqlProvider（連線測試、取得資料庫清單、取得資料表清單/結構/資料、產生備份/還原 CLI 命令）。ConnectionService 實作 CRUD 和測試連線，密碼存入時加密、讀取時解密。前端連線管理頁面：表格列出所有連線、新增/編輯對話框（含資料庫類型選擇、IP、Port、帳號、密碼、額外參數）、測試連線按鈕（即時回饋成功/失敗）、刪除確認。

**Blocked by:** 02 — 加密服務與工具偵測

**Status:** ready-for-agent

- [ ] IDbProvider 介面定義完成，DbProviderFactory 可正確解析 DbType 到 Provider
- [ ] MySqlProvider 單元測試通過（連線字串建構、備份/還原命令產生）
- [ ] ConnectionService 單元測試通過（CRUD + 密碼加解密整合）
- [ ] `POST /api/connections` 可建立連線（密碼加密儲存在 SQLite）
- [ ] `GET /api/connections` 回傳連線清單（密碼不外洩）
- [ ] `POST /api/connections/{id}/test` 回傳連線測試結果
- [ ] 前端連線管理頁面可新增/編輯/刪除/測試 MySQL 連線
- [ ] 表單驗證：必填欄位、Port 數值範圍
