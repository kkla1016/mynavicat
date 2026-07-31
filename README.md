# MyNavicat — SQL 資料庫備份與管理工具

MyNavicat 是一款媲美 Navicat 的現代化跨平台資料庫管理與自動化備份工具。支援包含 **MySQL, PostgreSQL, SQL Server, MariaDB, SQLite, Oracle** 在內的多種主流資料庫，具備連線管理、資料表瀏覽、即時與定時排程備份、超大型資料庫分片備份 (Chunked Backup)、資料匯出匯入 (CSV/JSON/SQL) 以及 WebSocket 桌面即時通知功能。

---

## 🚀 一鍵啟動 (One-Click Startup)

在專案根目錄下，您可以直接使用以下任一種方式一鍵啟動全套服務：

1. **直接雙擊執行**：雙擊根目錄下的 `start.bat`
2. **PowerShell 執行**：
   ```powershell
   .\start.ps1
   ```

執行後會自動開啟後端 API (Port 5050) 與前端 Vue 3 (Port 5173)，並自動打開預設瀏覽器跳轉至 `http://localhost:5173`。

---

## 🌟 核心功能

- 🔌 **多資料庫連線管理**：支援 MySQL、PostgreSQL、SQL Server、MariaDB、SQLite、Oracle。密碼採用 AES-256 對稱加密防護。
- 🔍 **資料表結構與內容瀏覽**：樹狀連線展覽、動態 DataTable 分頁預覽與 Schema 欄位結構檢視。
- 💾 **即時與分片備份 (Chunked Backup)**：呼叫原生 CLI 工具產出高可靠備份檔，支援 GZIP / ZIP 壓縮與大型庫分片切割。
- ⚡ **還原機制**：支援從歷史記錄一鍵還原或上傳外部 `.sql` / `.gz` / `.zip` 備份檔還原。
- ⏰ **Hangfire 定時排程備份**：Cron 視覺化編輯器設定週期排程、保留份數 (Retain Count) 自動清理過期檔案。
- 🔔 **SignalR 即時桌面通知**：排程完成或失敗時，透過 WebSocket 推送並觸發瀏覽器 Desktop Notification API 彈窗。
- 🛠️ **CLI 工具健康監控**：自動偵測 PATH 中的 `mysqldump`, `pg_dump`, `sqlcmd`, `sqlite3`, `exp` 工具可用性與版本。
- 📤 **資料匯出 / 匯入**：支援一鍵匯出或批次匯入 CSV / JSON / SQL INSERT 格式。

---

## 🛠️ 前置條件

1. **.NET 8.0 SDK** 或更高版本。
2. **Node.js 18+** 及 **npm**。
3. （可選）目標資料庫 CLI 工具已加入系統 PATH 變數：
   - MySQL / MariaDB: `mysqldump`, `mysql` / `mariadb-dump`, `mariadb`
   - PostgreSQL: `pg_dump`, `psql`
   - SQL Server: `sqlcmd`
   - SQLite: `sqlite3`
   - Oracle: `exp`, `imp`

---

## 🧪 執行單元測試

```bash
dotnet test backend/MyNavicat.Api.Tests/MyNavicat.Api.Tests.csproj
```

---

## 📁 專案結構

```
myvavicat/
├── start.bat                     # 一鍵啟動批次檔 (雙擊執行)
├── start.ps1                     # 一鍵啟動 PowerShell 腳本
├── backend/
│   ├── MyNavicat.Api/            # ASP.NET Core 8 Web API (Port 5050)
│   │   ├── Controllers/          # RESTful 控制器
│   │   ├── Data/                 # AppDbContext (SQLite)
│   │   ├── Hubs/                 # SignalR NotificationHub
│   │   ├── Models/               # Entities, DTOs, Common Response
│   │   ├── Providers/            # 6 大 DB Provider (IDbProvider)
│   │   └── Services/             # 業務層 (Backup, Schedule, Crypto 等)
│   └── MyNavicat.Api.Tests/      # xUnit 單元測試專案 (34 項測試)
├── frontend/                     # Vue 3 + Vite + TypeScript 前端 (Port 5173)
│   ├── src/
│   │   ├── api/                  # Axios API 請求模組
│   │   ├── components/           # UI 元件 (DatabaseTree, DataTable, CronEditor)
│   │   ├── composables/          # SignalR 與 Desktop Notification 組合式 API
│   │   ├── stores/               # Pinia 狀態管理 (Connection, Backup, Schedule)
│   │   ├── styles/               # SCSS 深色主題與 Element Plus 覆寫
│   │   └── views/                # 頁面視圖 (Connections, Browse, Backup, Restore, Schedules, History)
│   └── vite.config.ts            # Vite 設定與 API Proxy (指向 5050)
├── docs/
│   └── spec.md                   # 產品規格需求書 (PRD)
└── gemini.md                     # Antigravity 開發規範指引
```
