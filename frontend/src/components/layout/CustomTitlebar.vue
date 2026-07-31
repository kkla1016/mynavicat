<template>
  <div class="custom-titlebar">
    <div class="titlebar-drag-area">
      <img src="../../assets/vue.svg" alt="logo" class="app-logo" />
      <span class="app-title">MyNavicat — SQL 資料庫管理與備份工具</span>
    </div>

    <div class="titlebar-controls">
      <button class="control-btn minimize" title="最小化" @click="handleMinimize">
        <svg viewBox="0 0 1024 1024" width="12" height="12">
          <path d="M128 512h768v64H128z" fill="currentColor"/>
        </svg>
      </button>
      <button class="control-btn maximize" title="最大化 / 還原" @click="handleMaximize">
        <svg viewBox="0 0 1024 1024" width="12" height="12">
          <path d="M128 128h768v768H128V128z m64 64v640h640V192H192z" fill="currentColor"/>
        </svg>
      </button>
      <button class="control-btn close" title="隱藏至系統列" @click="handleClose">
        <svg viewBox="0 0 1024 1024" width="12" height="12">
          <path d="M576 512l288-288-64-64-288 288-288-288-64 64 288 288-288 288 64 64 288-288 288 288 64-64z" fill="currentColor"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
const handleMinimize = () => {
  if (window.electronAPI) window.electronAPI.minimizeWindow();
};

const handleMaximize = () => {
  if (window.electronAPI) window.electronAPI.maximizeWindow();
};

const handleClose = () => {
  if (window.electronAPI) window.electronAPI.closeWindow();
};
</script>

<style scoped lang="scss">
.custom-titlebar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 36px;
  background-color: #0b0f19;
  color: #94a3b8;
  font-size: 0.85rem;
  user-select: none;
  border-bottom: 1px solid #1e293b;

  .titlebar-drag-area {
    flex: 1;
    display: flex;
    align-items: center;
    padding-left: 12px;
    height: 100%;
    -webkit-app-region: drag;

    .app-logo {
      width: 18px;
      height: 18px;
      margin-right: 8px;
    }

    .app-title {
      font-weight: 500;
      color: #cbd5e1;
    }
  }

  .titlebar-controls {
    display: flex;
    height: 100%;
    -webkit-app-region: no-drag;

    .control-btn {
      display: flex;
      justify-content: center;
      align-items: center;
      width: 44px;
      height: 100%;
      background: transparent;
      border: none;
      color: #94a3b8;
      cursor: pointer;
      transition: background-color 0.15s ease, color 0.15s ease;

      &:hover {
        background-color: #1e293b;
        color: #f8fafc;
      }

      &.close:hover {
        background-color: #ef4444;
        color: #ffffff;
      }
    }
  }
}
</style>
