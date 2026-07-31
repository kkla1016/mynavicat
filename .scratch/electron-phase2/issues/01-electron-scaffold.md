# 01 — Electron 桌面骨架與 Preload 橋接

**What to build:** 配置 Electron 主進程 (Main Process)、Vite 開發伺服器整合與 Preload Script (`preload.ts`)。在 Electron 中透過 `contextBridge.exposeInMainWorld('electronAPI', ...)` 建立安全的 IPC 通道 API 簽名，並能成功以 `npm run electron:dev` 啟動深色風格 Frameless Electron 視窗。

**Blocked by:** None — can start immediately.

**Status:** ready-for-agent

- [ ] `npm run electron:dev` 可順利啟動 Electron 視窗並載入 Vue 3 前端
- [ ] 視窗採用 Frameless 無邊框模式，背景色配置為深色 (`#121824`)
- [ ] `preload.ts` 成功開放 `window.electronAPI` 給 Renderer Process 呼叫
- [ ] 前端 TypeScript 型別定義檔聲明 `window.electronAPI` 介面
- [ ] 能正確響應視窗基本生命週期與 IPC 通訊測試
