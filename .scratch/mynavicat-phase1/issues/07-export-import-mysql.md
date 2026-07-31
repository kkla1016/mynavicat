# 07 — 資料匯出入 — MySQL

**What to build:** 以 MySQL 為目標的資料匯出與匯入功能。匯出：使用者選擇連線 → 資料庫 → 資料表 → 選擇格式（CSV / Excel / JSON / SQL）→ 下載匯出檔案。匯入：使用者選擇連線 → 資料庫 → 目標資料表 → 上傳 CSV/Excel/JSON 檔案 → 系統解析並匯入資料 → 顯示匯入結果（成功筆數/失敗筆數）。

**Blocked by:** 04 — 資料表瀏覽 — MySQL

**Status:** ready-for-agent

- [ ] `POST /api/export` 以 CSV 格式匯出資料表資料
- [ ] `POST /api/export` 以 Excel 格式匯出資料表資料
- [ ] `POST /api/export` 以 JSON 格式匯出資料表資料
- [ ] `POST /api/export` 以 SQL INSERT 格式匯出資料表資料
- [ ] `GET /api/export/formats` 回傳支援的匯出格式清單
- [ ] `POST /api/import` 可接受 CSV 上傳並匯入到指定表
- [ ] `POST /api/import` 可接受 Excel 上傳並匯入到指定表
- [ ] `POST /api/import` 可接受 JSON 上傳並匯入到指定表
- [ ] 匯入結果回傳成功筆數和失敗筆數/原因
- [ ] ExportImportService 單元測試通過
- [ ] 前端匯出頁面可選擇格式後下載檔案
- [ ] 前端匯入頁面可上傳檔案並顯示匯入結果
