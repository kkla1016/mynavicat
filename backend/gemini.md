# MyNavicat 後端開發規範 (backend/gemini.md)

## 設計原則與架構規範

1. **S.O.L.I.D 原則**:
   - **Single Responsibility**: 控制器 (Controllers) 僅處理 HTTP 路由與回應，業務邏輯下沉至 Services。
   - **Open/Closed & Dependency Inversion**: 資料庫 Provider 採用 `IDbProvider` 介面 + `DbProviderFactory` 工廠模式，新資料庫支援僅需擴充新 Provider 無須修改現有邏輯。
2. **RESTful 風格 API**:
   - 資源路徑複數命名 (`/api/connections`, `/api/schedules`, `/api/backup/history`)。
   - 統一回應包裝 `ApiResponse<T>`。
3. **程式碼註解規範**:
   - 類別與公用 API 必須附帶 XML 中文註解 (`/// <summary>`)。
4. **測試先行 (TDD)**:
   - 任何 Service 修改前或新增後必須覆蓋 xUnit 單元測試 (位於 `MyNavicat.Api.Tests`)。
