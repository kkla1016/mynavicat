# 06 — 資料庫與資料表結構/內容預覽 (Electron)

**What to build:** 經由 IPC 通訊提供資料庫結構與內容瀏覽功能。Main Process 實作 `GetTablesAsync`, `GetTableSchemaAsync`, `GetTableDataAsync`。前端 `DatabaseTree.vue` 展現樹狀連線-庫-表結構，`BrowseView.vue` 與 `DataTable.vue` 提供分頁預覽與 Schema 欄位（名稱、型別、Null、Primary Key）檢視。

**Blocked by:** 05 — 桌面連線管理與 MySqlProvider (Electron)

**Status:** ready-for-agent

- [ ] 樹狀元件點擊連線可展開該伺服器上的所有資料庫與資料表
- [ ] 選擇資料表後，Schema Tab 正確展示欄位名稱、資料型別與 Primary Key
- [ ] 選擇資料表後，Data Tab 正確分頁顯示資料列 (DataTable)
- [ ] 分頁控制器切換頁碼時可正確載入對應頁面的資料
