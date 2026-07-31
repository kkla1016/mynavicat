# 02 — 加密服務與工具偵測

**What to build:** 實作兩個跨功能基礎服務。(1) CryptoService：AES-256-CBC 對稱加解密，用於保護資料庫連線密碼。金鑰從 DPAPI 或應用程式設定取得。(2) ToolDetectionService：偵測系統 PATH 中的各資料庫 CLI 工具（mysqldump、pg_dump、sqlcmd、sqlite3、exp），回報每個工具的可用性和版本。前端 Header 顯示工具狀態燈號（綠/紅），點擊可查看詳細清單，未安裝的工具提供安裝指引連結。

**Blocked by:** 01 — 專案骨架與基礎設施

**Status:** ready-for-agent

- [ ] CryptoService 的加密/解密結果對稱（加密後解密回原文）
- [ ] CryptoService 單元測試覆蓋空字串、中文字、特殊字元、長字串
- [ ] `GET /api/tools/status` 回傳各工具的名稱、可用性、版本、建議安裝指引
- [ ] ToolDetectionService 單元測試驗證偵測邏輯
- [ ] 前端 Header 顯示工具狀態燈號，可展開查看詳細清單
- [ ] 未安裝的工具在前端顯示安裝指引提示
