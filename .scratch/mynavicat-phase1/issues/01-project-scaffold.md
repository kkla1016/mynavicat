# 01 — 專案骨架與基礎設施

**What to build:** 建立 MyNavicat 的完整專案骨架。後端：ASP.NET Core 8 Web API 專案，設定好 DI、CORS（允許前端 5173）、Swagger、Serilog 日誌、SQLite（EF Core）資料庫（包含 Connection、BackupHistory、BackupSchedule 三個實體的 Migration）、Hangfire 排程引擎（SQLite storage）、SignalR Hub 端點。前端：Vue 3 + Vite + TypeScript 專案，安裝 Element Plus、Pinia、Vue Router、Axios、SCSS，設定深色主題樣式變數和 Element Plus dark mode 覆寫，建立 AppLayout（Sidebar 導航 + Header）骨架和 Vue Router 路由。兩端可分別啟動並透過 Vite proxy 串通 API。

**Blocked by:** None — can start immediately.

**Status:** ready-for-agent

- [ ] `dotnet run` 可啟動後端並存取 Swagger UI
- [ ] EF Core Migration 產生 SQLite 資料庫包含三個實體的表
- [ ] Hangfire Dashboard 可在 `/hangfire` 存取
- [ ] SignalR Hub 端點 `/hubs/notification` 可建立 WebSocket 連線
- [ ] `npm run dev` 可啟動前端
- [ ] 前端深色主題 Layout（Sidebar + Header + 空白主內容區）正確顯示
- [ ] 前端透過 Vite proxy 可成功呼叫後端 Swagger 端點
- [ ] 後端單元測試專案（xUnit）可編譯執行
