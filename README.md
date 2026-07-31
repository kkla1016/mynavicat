# MyNavicat — 桌面單機 SQL 資料庫備份與管理工具 (Electron 桌面版)

MyNavicat 是一款媲美 Navicat 的現代化跨平台 SQL 資料庫管理與自動化備份工具。採用 **Electron + Vue 3 + TypeScript + Better-SQLite3** 全單機架構開發，支援 **MySQL, PostgreSQL, SQL Server, MariaDB, SQLite, Oracle** 六大主流資料庫。

---

## 🌟 核心特色 (桌面單機版)

- 💻 **純單機桌面應用**：雙擊即可啟動，無需啟動 Web 伺服器，無 Port 衝突問題。
- 🖼️ **深色主題 Frameless 視窗**：客製化深色標題列與視覺風格。
- 📌 **System Tray 系統列常駐**：關閉主視窗自動縮小至系統列，確保 Hangfire/Node-Cron 排程備份持續運行。
- 🔔 **Windows 原生系統通知**：備份完成或失敗時，直接透過 Windows 通知中心跳出提醒。
- 🔌 **6 大資料庫連線管理**：支援 MySQL、PostgreSQL、SQL Server、MariaDB、SQLite、Oracle。密碼採用 AES-256 對稱加密防護。
- 🔍 **資料表結構與資料瀏覽**：樹狀連線展覽、動態 DataTable 分頁預覽與 Schema 欄位結構檢視。
- 💾 **即時與分片備份 (Chunked Backup)**：呼叫原生 CLI 工具產出高可靠備份檔，支援 GZIP / ZIP 壓縮與大型庫分片切割。
- 📤 **資料匯出 / 匯入**：支援一鍵匯出或批次匯入 CSV / JSON / SQL INSERT 格式。

---

## 🛠️ 開發與建置

### 前置條件
- **Node.js 18+** & **npm**

### 開發模式
```bash
npm install
npm run electron:dev
```

### 打包 Windows NSIS 安裝包 (.exe)
```bash
npm run electron:build
```
產出的安裝包位於 `dist_electron/` 目錄。

---

## 📁 專案結構

```
myvavicat/
├── src/                          # Electron Main Process & Native Logic
│   ├── main/                     # Electron 主進程 (Window, Tray, IPC)
│   ├── preload/                  # Preload contextBridge 橋接 API
│   ├── services/                 # 核心業務邏輯 (Backup, Schedule, DB Providers, SQLite Storage)
│   └── providers/                # 6 大 DB Provider 實作
├── frontend/                     # Vue 3 UI 渲染層 (Renderer Process)
│   ├── src/
│   │   ├── components/           # Titlebar, DatabaseTree, DataTable 等
│   │   ├── stores/               # Pinia Stores
│   │   └── views/                # 介面視圖
└── docs/
    └── spec.md                   # 產品規格需求書 (含 Mermaid 系統架構圖)
```
