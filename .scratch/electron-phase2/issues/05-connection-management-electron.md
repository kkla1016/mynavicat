# 05 — 桌面連線管理與 MySqlProvider (Electron)

**What to build:** 定義 Node.js `IDbProvider` 介面與 `DbProviderFactory`。實作 `MySqlProvider`（選用 `mysql2` 驅動處理連線測試，並生成 `mysqldump` / `mysql` 命令）。前端連線管理介面 (ConnectionsView) 完全改用 `window.electronAPI` 與 Main Process 溝通，進行連線 CRUD、密碼遮蔽與測試連線。

**Blocked by:** 04 — AES-256 加密與 CLI 工具監控 (Electron)

**Status:** ready-for-agent

- [ ] `MySqlProvider` 成功使用 `mysql2` 連接 MySQL 資料庫並回傳測試結果
- [ ] 能正確透過 `mysql2` 擷取伺服器上的資料庫清單
- [ ] 前端 `ConnectionsView.vue` 可新增、編輯、刪除連線並顯示列表
- [ ] 密碼儲存於 SQLite 時為 AES-256 密文，前端展示時自動遮蔽
- [ ] 點擊「測試連線」可於 UI 收到即時成功/失敗反饋
