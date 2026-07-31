# 04 — AES-256 加密與 CLI 工具監控 (Electron)

**What to build:** (1) Node.js `crypto` 模組實作 AES-256-CBC 對稱加解密，替換原先 C# 的 CryptoService。(2) ToolDetectionService：使用 Node.js `child_process.exec` 偵測 PATH 中的各資料庫 CLI 工具 (`mysqldump`, `pg_dump`, `sqlcmd`, `sqlite3`, `exp`)可用性與版本。(3) 前端 Header 燈號與展開對話框串接 IPC，即時回饋各 CLI 工具狀態。

**Blocked by:** 03 — Better-SQLite3 本地儲存模組

**Status:** ready-for-agent

- [ ] Node.js CryptoService 的加解密結果對稱（明文 -> 密文 -> 明文）
- [ ] 能正確認出系統 PATH 中的 `mysqldump`, `pg_dump` 等工具與版本號
- [ ] 前端 Header 成功透過 IPC 取得 CLI 工具清單與可用狀態
- [ ] 未安裝的 CLI 工具在 UI 上顯示安裝建議指引連結
- [ ] 寫入連線密碼時經由 cryptoService 加密後存入 `app.db`
