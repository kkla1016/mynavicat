# 04 — 資料表瀏覽 — MySQL

**What to build:** 以 MySQL 連線為目標，實作資料表瀏覽功能。左側 DatabaseTree 元件以樹狀結構展示：連線 → 資料庫 → 資料表。點選資料表後右側顯示兩個分頁：(1) 結構分頁 — 欄位名稱、型別、是否 NULL、預設值、索引等。(2) 資料分頁 — 以 DataTable 元件分頁預覽表內資料（支援翻頁和每頁筆數設定）。

**Blocked by:** 03 — 連線管理（CRUD + 測試連線）— MySQL

**Status:** ready-for-agent

- [ ] `GET /api/browse/{connectionId}/databases` 回傳 MySQL 資料庫清單
- [ ] `GET /api/browse/{connectionId}/{db}/tables` 回傳資料表清單
- [ ] `GET /api/browse/{connectionId}/{db}/{table}/schema` 回傳欄位定義
- [ ] `GET /api/browse/{connectionId}/{db}/{table}/data?page=1&pageSize=50` 回傳分頁資料
- [ ] DatabaseBrowseService 單元測試通過
- [ ] 前端左側 DatabaseTree 正確顯示樹狀結構
- [ ] 點選資料表後右側結構分頁顯示欄位定義
- [ ] 點選資料表後右側資料分頁顯示分頁資料
