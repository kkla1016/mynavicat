<template>
  <header class="app-header">
    <div class="header-left">
      <span class="page-title">{{ currentRouteName }}</span>
    </div>
    <div class="header-right">
      <div class="tool-status" @click="dialogVisible = true" style="cursor: pointer;">
        <el-tag :type="overallStatusType" effect="dark" round size="small">
          <el-icon style="margin-right: 4px;"><Tools /></el-icon>
          CLI 工具 ({{ availableCount }}/{{ tools.length }})
        </el-tag>
      </div>
      <div class="version-tag">
        v1.0.0 Phase 1
      </div>
    </div>

    <!-- CLI 工具狀態詳細對話框 -->
    <el-dialog v-model="dialogVisible" title="系統 CLI 工具偵測狀態" width="600px">
      <el-table :data="tools" style="width: 100%" v-loading="loading">
        <el-table-column prop="displayName" label="工具名稱" width="180" />
        <el-table-column prop="targetDbType" label="對應資料庫" width="120" />
        <el-table-column label="狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="row.isAvailable ? 'success' : 'danger'" size="small">
              {{ row.isAvailable ? '已安裝' : '未偵測到' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="版本 / 操作">
          <template #default="{ row }">
            <span v-if="row.isAvailable" class="version-text">{{ row.version }}</span>
            <el-link v-else type="primary" :href="row.installGuideUrl" target="_blank">
              安裝指引 <el-icon><TopRight /></el-icon>
            </el-link>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>
  </header>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { Tools, TopRight } from '@element-plus/icons-vue'
import axios from 'axios'

interface ToolStatus {
  name: string
  displayName: string
  targetDbType: string
  isAvailable: boolean
  version: string | null
  installGuideUrl: string
}

const route = useRoute()
const dialogVisible = ref(false)
const tools = ref<ToolStatus[]>([])
const loading = ref(false)

const availableCount = computed(() => tools.value.filter(t => t.isAvailable).length)
const overallStatusType = computed(() => {
  if (availableCount.value === tools.value.length && tools.value.length > 0) return 'success'
  if (availableCount.value > 0) return 'warning'
  return 'danger'
})

const fetchToolsStatus = async () => {
  loading.value = true
  try {
    const res = await axios.get('/api/tools/status')
    if (res.data && res.data.success) {
      tools.value = res.data.data
    }
  } catch (err) {
    console.error('Failed to fetch tool status', err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchToolsStatus()
})

const currentRouteName = computed(() => {
  switch (route.path) {
    case '/connections': return '連線管理'
    case '/browse': return '資料表瀏覽'
    case '/backup': return '資料庫備份'
    case '/restore': return '資料庫還原'
    case '/schedules': return '排程管理'
    case '/history': return '備份歷史記錄'
    case '/export-import': return '資料匯出 / 匯入'
    default: return 'MyNavicat'
  }
})
</script>

<style scoped lang="scss">
.app-header {
  height: 56px;
  background-color: #1a2234;
  border-bottom: 1px solid #2e3a52;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;

  .header-left {
    .page-title {
      font-size: 1.1rem;
      font-weight: 600;
      color: #e2e8f0;
    }
  }

  .header-right {
    display: flex;
    align-items: center;
    gap: 16px;

    .version-tag {
      font-size: 0.85rem;
      color: #64748b;
    }
  }

  .version-text {
    font-size: 0.85rem;
    color: #94a3b8;
    word-break: break-all;
  }
}
</style>
