# 10 — 6 大資料庫 Provider 與 NSIS 安裝包

**What to build:** (1) 補齊其餘 5 種資料庫 Node.js Provider 實作 (`PostgreSqlProvider`, `SqlServerProvider`, `MariaDbProvider`, `SqliteProvider`, `OracleProvider`)，確保 `DbProviderFactory` 能解析 6 大資料庫連線、瀏覽、備份與還原。(2) 配置 `electron-builder.json`，使用 `electron-builder` 打包出產 Windows **NSIS (.exe)** 安裝程式 (含桌面捷徑與解安裝精靈)。

**Blocked by:** 09 — 桌面版資料匯出入 CSV / JSON / SQL (Electron)

**Status:** ready-for-agent

- [ ] 6 大資料庫 Provider 皆完成 Node.js 實作與連線測試、CLI 備份/還原命令生成
- [ ] 寫入全套 6 大資料庫 Provider 的單元測試並全數通過
- [ ] `npm run electron:build` 成功打包出產 `.exe` NSIS 安裝檔
- [ ] 安裝檔能於乾淨 Windows 環境順利安裝、建立桌面捷徑並開啟使用
- [ ] 支援在 Windows 控制台 / 設定中進行完整解安裝
