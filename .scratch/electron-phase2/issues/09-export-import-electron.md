# 09 — 桌面版資料匯出入 CSV / JSON / SQL (Electron)

**What to build:** 實作單機桌面版的資料表數據匯出與匯入功能。(1) 匯出：Main Process 讀取資料表並格式化為 CSV、JSON 或 SQL INSERT 語句，搭配 Electron `dialog.showSaveDialog` 讓使用者選擇儲存位置。(2) 匯入：搭配 `dialog.showOpenDialog` 選擇 CSV 或 JSON 檔案，Main Process 讀取檔案內容並批次寫入目標資料庫。(3) 前端 `ExportImportView.vue` 畫面串接。

**Blocked by:** 06 — 資料庫與資料表結構/內容預覽 (Electron)

**Status:** ready-for-agent

- [ ] 選擇連線與資料表後點擊匯出，可觸發 Windows 原生存檔對話框
- [ ] 匯出的 CSV, JSON, SQL 檔案內容格式正確無誤
- [ ] 選擇 CSV 或 JSON 檔案進行匯入，能批次寫入目標資料表
- [ ] 前端 UI 能正確顯示匯入成功筆數與失敗筆數摘要
