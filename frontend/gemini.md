# MyNavicat 前端開發規範 (frontend/gemini.md)

## 設計與開發規範

1. **技術選型**:
   - Vue 3 + Composition API (`<script setup lang="ts">`)
   - TypeScript 強型別檢查與分層型別定義 (`src/types/`)
   - Element Plus UI 元件庫（深色模式 `:dark`）
   - SCSS 全域設計代幣與主題變數 (`src/styles/variables.scss`)
2. **設計美學**:
   - 採用專業深色主題（`#121824` 核心基底，`#1a2234` 側欄，`#1d263b` 卡片）。
   - 避免預設色彩，採用調和色盤與平滑轉場。
3. **即時互動**:
   - SignalR 廣播監聽 + Web Notification API 即時桌面彈窗提醒。
