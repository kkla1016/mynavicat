<template>
  <header class="app-header">
    <div class="header-left">
      <h2>MyNavicat 資料庫管理與自動化備份</h2>
    </div>

    <div class="header-right">
      <el-tooltip content="查看系統 CLI 工具可用狀態" placement="bottom">
        <div class="tool-status-badge" @click="dialogVisible = true">
          <span class="status-dot" :class="{ ok: allToolsOk, warning: !allToolsOk }"></span>
          <span class="status-text">CLI 工具狀態 ({{ availableCount }}/{{ tools.length }})</span>
        </div>
      </el-tooltip>

      <el-dialog v-model="dialogVisible" title="系統 CLI 工具偵測報告" width="600px">
        <el-table :data="tools" stripe style="width: 100%">
          <el-table-column prop="name" label="工具名稱" width="160" />
          <el-table-column prop="executable" label="執行檔" width="120" />
          <el-table-column label="狀態" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.isAvailable ? 'success' : 'danger'">
                {{ scope.row.isAvailable ? '可用' : '未安裝' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="version" label="版本資訊" />
          <el-table-column label="指引" width="90">
            <template #default="scope">
              <el-link v-if="!scope.row.isAvailable" type="primary" :href="scope.row.installGuideUrl" target="_blank">
                安裝指引
              </el-link>
            </template>
          </el-table-column>
        </el-table>

        <template #footer>
          <el-button type="primary" @click="fetchToolsStatus">重新檢測</el-button>
          <el-button @click="dialogVisible = false">關閉</el-button>
        </template>
      </el-dialog>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

const dialogVisible = ref(false)
const tools = ref<any[]>([])

const availableCount = computed(() => tools.value.filter(t => t.isAvailable).length)
const allToolsOk = computed(() => availableCount.value === tools.value.length && tools.value.length > 0)

const fetchToolsStatus = async () => {
  if (window.electronAPI) {
    try {
      tools.value = await window.electronAPI.getToolsStatus()
    } catch {
      tools.value = []
    }
  }
}

onMounted(() => {
  fetchToolsStatus()
})
</script>

<style scoped lang="scss">
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 50px;
  padding: 0 20px;
  background-color: #161e2e;
  border-bottom: 1px solid #2e3a52;

  .header-left {
    h2 {
      font-size: 1rem;
      font-weight: 500;
      color: #e2e8f0;
      margin: 0;
    }
  }

  .header-right {
    .tool-status-badge {
      display: flex;
      align-items: center;
      padding: 4px 12px;
      background-color: #1d263b;
      border: 1px solid #2e3a52;
      border-radius: 16px;
      cursor: pointer;
      font-size: 0.85rem;
      color: #94a3b8;
      transition: all 0.2s ease;

      &:hover {
        border-color: #3b82f6;
        color: #f8fafc;
      }

      .status-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        margin-right: 8px;

        &.ok {
          background-color: #10b981;
          box-shadow: 0 0 6px rgba(16, 185, 129, 0.6);
        }

        &.warning {
          background-color: #f59e0b;
          box-shadow: 0 0 6px rgba(245, 158, 11, 0.6);
        }
      }
    }
  }
}
</style>
