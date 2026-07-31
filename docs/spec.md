# MyNavicat — 資料庫備份管理工具 Spec

## Problem Statement

DBA 和開發者日常需要對多種資料庫（MySQL、PostgreSQL、SQL Server、MariaDB、SQLite、Oracle）進行備份、還原、排程維護、資料瀏覽與匯出入等操作。市面上的工具如 Navicat 功能完整但價格昂貴且為桌面應用程式，難以在團隊中快速部署或跨設備存取。使用者需要一個免費、開源、可透過瀏覽器操作的替代方案，能夠指定遠端 IP 進行資料庫連線與備份管理。

## Solution

建立 **MyNavicat**，一個 Web 版資料庫備份管理工具。使用者透過瀏覽器存取前端介面，即可管理多個資料庫連線、執行備份與還原、設定定時排程備份、瀏覽資料表結構與資料、以及進行資料匯出入。系統透過呼叫各資料庫的原生 CLI 工具（mysqldump、pg_dump、sqlcmd 等）確保備份的完整性與可靠度，並在排程備份完成或失敗時推送瀏覽器桌面通知。

分兩階段交付：
- **第一階段**：連線管理、備份/還原、排程備份、備份歷史、資料表瀏覽、資料匯出入、桌面通知
- **第二階段**：SQL 查詢編輯器（Monaco Editor）、自動完成、查詢歷史、多分頁查詢、查詢結果匯出

## User Stories

### 連線管理
1. 身為 DBA，我想要新增一個資料庫連線並指定 IP、Port、帳號密碼，以便我能連接到遠端資料庫伺服器。
2. 身為 DBA，我想要選擇資料庫類型（MySQL / PostgreSQL / SQL Server / MariaDB / SQLite / Oracle），以便系統使用正確的驅動和 CLI 工具。
3. 身為 DBA，我想要測試一個連線是否正常，以便在儲存前確認設定正確。
4. 身為 DBA，我想要編輯已儲存的連線資訊，以便在 IP 或帳號變更時更新設定。
5. 身為 DBA，我想要刪除不再需要的連線，以便保持連線清單整潔。
6. 身為 DBA，我想要我的資料庫密碼被加密儲存，以便避免明文密碼洩漏的安全風險。
7. 身為 DBA，我想要在連線表單中設定額外參數（如 charset、SSL 等），以便適應各種伺服器設定。

### 資料庫備份
8. 身為 DBA，我想要選擇一個連線後看到該伺服器上所有資料庫清單，以便選擇要備份的資料庫。
9. 身為 DBA，我想要對整個資料庫執行完整備份，以便取得可用於災難復原的備份檔。
10. 身為 DBA，我想要只備份指定的資料表，以便僅備份需要的資料而非整個資料庫。
11. 身為 DBA，我想要選擇是否壓縮備份檔（gz / zip），以便節省磁碟空間。
12. 身為 DBA，我想要在備份過程中看到進度指示，以便知道備份是否仍在進行中。
13. 身為 DBA，我想要備份失敗時看到明確的錯誤訊息，以便快速排查問題。
14. 身為 DBA，我想要大型資料庫能夠自動分片備份，以便避免產生過大的單一備份檔造成管理困難。

### 資料庫還原
15. 身為 DBA，我想要從備份歷史記錄中選擇一筆備份來還原，以便快速恢復資料庫到先前的狀態。
16. 身為 DBA，我想要上傳外部的備份檔案來執行還原，以便恢復從其他來源取得的備份。
17. 身為 DBA，我想要選擇還原到不同的目標連線或資料庫，以便在測試環境中重建資料。
18. 身為 DBA，我想要壓縮的備份檔能自動解壓後再還原，以便不需要手動解壓。

### 排程備份
19. 身為 DBA，我想要設定一個 Cron 排程來自動執行資料庫備份，以便不需要手動定期操作。
20. 身為 DBA，我想要為排程選擇連線、資料庫、資料表、壓縮選項，以便精確控制自動備份的範圍。
21. 身為 DBA，我想要設定排程的備份保留份數，以便自動清理過舊的備份檔節省空間。
22. 身為 DBA，我想要啟用或停用排程而不刪除它，以便暫時暫停自動備份。
23. 身為 DBA，我想要手動立即觸發一次排程執行，以便測試排程設定是否正確。
24. 身為 DBA，我想要在排程管理頁面看到每個排程的下次預計執行時間，以便掌握備份時程。

### 桌面通知
25. 身為 DBA，我想要排程備份成功時收到瀏覽器桌面通知，以便即使不盯著頁面也能知道備份已完成。
26. 身為 DBA，我想要排程備份失敗時收到瀏覽器桌面通知並看到錯誤摘要，以便立即處理問題。

### 備份歷史
27. 身為 DBA，我想要查看所有備份的歷史記錄（含狀態、時間、大小、耗時），以便追蹤備份情況。
28. 身為 DBA，我想要依連線、資料庫、狀態、時間範圍篩選備份歷史，以便快速找到特定記錄。
29. 身為 DBA，我想要下載備份歷史中的備份檔案，以便手動移至其他位置保存。
30. 身為 DBA，我想要刪除不需要的備份歷史記錄及其檔案，以便釋放磁碟空間。

### 資料表瀏覽
31. 身為開發者，我想要以樹狀結構瀏覽一個連線下的資料庫和資料表，以便快速概覽資料庫結構。
32. 身為開發者，我想要查看資料表的欄位定義（名稱、型別、是否允許 NULL、預設值、索引等），以便了解資料表結構。
33. 身為開發者，我想要預覽資料表中的資料（含分頁），以便確認資料內容。

### 資料匯出入
34. 身為開發者，我想要將資料表的資料匯出為 CSV / Excel / JSON / SQL 格式，以便在其他工具或報表中使用。
35. 身為開發者，我想要從 CSV / Excel / JSON 檔案匯入資料到指定資料表，以便批次匯入測試資料或遷移資料。

### 工具偵測
36. 身為 DBA，我想要在系統啟動或設定頁面中看到各 CLI 工具（mysqldump、pg_dump 等）的偵測結果，以便知道系統支援哪些資料庫的備份。
37. 身為 DBA，我想要未偵測到的工具有安裝指引提示，以便快速完成設定。

### 整體體驗
38. 身為 DBA，我想要介面使用深色主題，以便長時間使用時減少眼睛疲勞。

## Implementation Decisions

### 架構與選型
- **前後端分離架構**：後端 ASP.NET Core 8 Web API，前端 Vue 3 + Vite + TypeScript + Element Plus。
- **App 設定資料儲存**：使用嵌入式 SQLite 資料庫，零配置、不需額外安裝。
- **排程引擎**：使用 Hangfire（搭配 SQLite storage），提供內建的 Dashboard、重試機制和任務監控。
- **即時通訊**：使用 ASP.NET Core SignalR 建立 WebSocket 連線，後端推送備份事件到前端。
- **前端狀態管理**：Pinia。路由：Vue Router。HTTP 客戶端：Axios。

### 安全
- **密碼加密**：資料庫連線密碼使用 AES-256-CBC 加密儲存。金鑰透過 DPAPI（Windows）或應用程式設定取得。
- **無使用者認證**：單人使用場景，不實作登入系統。

### 備份機制
- **原生 CLI 工具呼叫**：透過 `System.Diagnostics.Process` 啟動各資料庫的原生 CLI 工具（mysqldump、pg_dump、sqlcmd/bcp、sqlite3、exp/expdp），以確保備份的完整性和相容性。
- **分片備份**：大型資料庫不設單次備份大小上限，採用按資料表分片產生多個備份檔的策略。BackupHistory 以 `ParentBackupId`、`ChunkIndex`、`TotalChunks` 欄位記錄分片關係。
- **壓縮**：支援 gz 和 zip 兩種壓縮格式。
- **備份保留**：排程備份可設定保留份數，超出後自動刪除最舊的備份檔案與記錄。

### 通知
- **SignalR Hub**：後端 `NotificationHub` 透過 `/hubs/notification` 端點，以 `BackupCompleted` / `BackupFailed` 事件推送到前端。
- **Browser Notification API**：前端收到 SignalR 事件後，呼叫 Browser Notification API 推送桌面通知（需使用者首次授權）。

### 資料庫 Provider 設計
- **策略模式**：`IDbProvider` 介面 + `DbProviderFactory` 工廠模式，每種資料庫實作獨立的 Provider（MySqlProvider、PostgreSqlProvider、SqlServerProvider、MariaDbProvider、SqliteProvider、OracleProvider）。
- **MariaDB 與 MySQL 共用協議**：MariaDbProvider 繼承/擴展 MySqlProvider。

### API 設計
- **RESTful 風格**：6 個 Controller（Connection、Backup、Schedule、Browse、ExportImport、ToolDetection），統一使用 `ApiResponse<T>` 回應格式。

### 部署
- **本機直接運行**：後端 `dotnet run`（Port 5000）、前端 `npm run dev`（Port 5173），Vite proxy 轉發 API 請求到後端。

## Testing Decisions

### 什麼是好的測試
- 只測試**外部行為**（公開介面的輸入/輸出），不測試內部實作細節。
- 測試應該是**可靠且可重複**的，不依賴真實的外部資料庫連線（使用 mock）。
- 每個測試只驗證**一個行為**，失敗時能清楚指出哪個行為出問題。

### 主要測試接縫
1. **Service Layer 接縫**（主要接縫）：所有核心邏輯集中在 Service 層。透過 DI 注入 mock 的 `IDbProvider`、`IHubContext<NotificationHub>`、`AppDbContext`（使用 InMemory Provider），可完全隔離外部依賴進行單元測試。
2. **API Controller 接縫**：透過 `WebApplicationFactory` 做整合測試，驗證路由對應、請求驗證和回應格式。
3. **前端 API 模組接縫**：Axios 實例可被 mock，用於元件測試時隔離後端依賴。

### 要測試的模組
- `CryptoService`：加密/解密對稱性、不同輸入長度、金鑰管理
- `ConnectionService`：CRUD 操作、密碼加解密整合、TestConnection
- `BackupService`：備份命令產生、壓縮流程、分片邏輯、歷史記錄管理
- `ScheduleService`：排程 CRUD、Hangfire Job 註冊/取消、保留份數清理
- `NotificationService`：事件推送至 SignalR Hub
- `ToolDetectionService`：CLI 工具路徑偵測
- 各 `IDbProvider` 實作：連線字串建構、備份/還原命令產生
- `DbProviderFactory`：工廠正確解析 DbType 到 Provider

### 測試框架
- 後端：xUnit + Moq + FluentAssertions
- 前端 E2E：Playwright

## Out of Scope

以下項目**不在第一階段範圍內**，規劃於第二階段或後續版本實作：

1. **SQL 查詢編輯器**（Monaco Editor、語法高亮、自動完成）
2. **查詢歷史記錄**
3. **多分頁查詢**
4. **查詢結果匯出**
5. **使用者認證與多人權限管理**
6. **雲端備份儲存**（S3 / Azure Blob / GCS）
7. **FTP / SFTP 遠端上傳**
8. **Docker 容器化部署**
9. **SSH 通道連線**（已在資料模型中預留欄位，但不實作）
10. **資料庫結構比較與同步**
11. **資料遷移工具**
12. **Email 通知**

## Further Notes

- **Oracle 支援優先級較低**：Oracle 備份/還原需要安裝 Oracle Instant Client，有額外的授權需求。第一版以其他五種資料庫為主，Oracle 隨後補強。
- **CLI 工具為前置條件**：系統運行需要在 PATH 中安裝對應的資料庫 CLI 工具。系統啟動時會自動偵測，並在 UI 中提示未安裝的工具。
- **深色主題**：前端 UI 採用深色主題設計，使用 Element Plus 的 dark mode 配置搭配自訂 SCSS 變數。
- **SOLID 原則**：後端程式碼遵循 S.O.L.I.D 設計原則，特別是 SRP（每個 Service 職責單一）和 DIP（依賴介面而非實作）。
- **中文註解**：所有程式碼的函式級別註解使用中文，重要變數也加上中文註解。
