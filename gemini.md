# MyNavicat 主專案開發規範與架構指引

## 專案整體架構

本專案是一個類似 Navicat 的 SQL 資料庫備份與管理工具，分為前後端獨立架構：
- **後端**: ASP.NET Core 8 Web API, EF Core (SQLite 本地元資料庫), Hangfire 排程引擎, SignalR 即時 WebSocket 推播。
- **前端**: Vue 3, Vite, TypeScript, Element Plus (深色主題), Pinia 狀態管理, Vue Router, Axios。

## 開發與部署慣例

1. **本地雙端啟動**:
   - 後端: `dotnet run` (運作於 Port 5000)
   - 前端: `npm run dev` (運作於 Port 5173，Proxy 將 `/api` 與 `/hubs` 轉發給 Port 5000)
2. **規格一致性**:
   - 所有異動前必須優先查閱 `docs/spec.md` 與 `implementation_plan.md`。
   - 保留繁體中文溝通與程式碼中文註解原則。
