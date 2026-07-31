<template>
  <div class="page-container">
    <div class="filter-bar">
      <el-select
        v-model="statusFilter"
        placeholder="狀態篩選"
        clearable
        style="width: 140px;"
        @change="handleFilter"
      >
        <el-option label="Success" value="Success" />
        <el-option label="Failed" value="Failed" />
        <el-option label="InProgress" value="InProgress" />
      </el-select>

      <el-button :icon="Refresh" @click="handleFilter">重新整理</el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="backupStore.historyList" v-loading="backupStore.loading" style="width: 100%">
        <el-table-column prop="databaseName" label="資料庫" width="140" />
        <el-table-column prop="backupType" label="備份類型" width="110">
          <template #default="{ row }">
            <el-tag size="small" type="info">{{ row.backupType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="檔案大小" width="120">
          <template #default="{ row }">
            {{ formatFileSize(row.fileSize) }}
          </template>
        </el-table-column>
        <el-table-column label="壓縮" width="90">
          <template #default="{ row }">
            <el-tag v-if="row.isCompressed" size="small" type="warning">{{ row.compressionType }}</el-tag>
            <span v-else class="text-muted">無</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="狀態" width="120">
          <template #default="{ row }">
            <el-tag :type="getStatusTagType(row.status)">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="開始時間" min-width="170">
          <template #default="{ row }">
            {{ formatDate(row.startedAt) }}
          </template>
        </el-table-column>
        <el-table-column prop="durationSeconds" label="耗時 (秒)" width="100">
          <template #default="{ row }">
            {{ row.durationSeconds ? row.durationSeconds.toFixed(1) + ' s' : '-' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="row.status === 'Success'"
              size="small"
              type="primary"
              plain
              @click="handleDownload(row.id)"
            >
              下載
            </el-button>
            <el-popconfirm title="確定刪除此歷史記錄與備份檔案嗎？" @confirm="handleDelete(row.id)">
              <template #reference>
                <el-button size="small" type="danger" plain>刪除</el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-bar">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          layout="total, prev, pager, next"
          :total="backupStore.totalCount"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Refresh } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { useBackupStore } from '../stores/backupStore'
import { getBackupDownloadUrl } from '../api/backup'

const backupStore = useBackupStore()

const statusFilter = ref<string | undefined>(undefined)
const page = ref(1)
const pageSize = ref(20)

onMounted(() => {
  handleFilter()
})

const handleFilter = () => {
  backupStore.fetchHistory(undefined, statusFilter.value, page.value, pageSize.value)
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  handleFilter()
}

const getStatusTagType = (status: string) => {
  switch (status) {
    case 'Success': return 'success'
    case 'Failed': return 'danger'
    case 'InProgress': return 'warning'
    default: return 'info'
  }
}

const formatFileSize = (bytes: number) => {
  if (!bytes || bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString()
}

const handleDownload = (id: number) => {
  const url = getBackupDownloadUrl(id)
  window.open(url, '_blank')
}

const handleDelete = async (id: number) => {
  const res = await backupStore.deleteHistory(id)
  if (res && res.success) {
    ElMessage.success('已刪除歷史記錄與對應檔案')
  }
}
</script>

<style scoped lang="scss">
.page-container {
  padding: 24px;

  .filter-bar {
    margin-bottom: 16px;
    display: flex;
    gap: 12px;
  }

  .table-card {
    background-color: #1d263b;
    border-color: #2e3a52;

    .text-muted {
      color: #64748b;
    }

    .pagination-bar {
      margin-top: 16px;
      display: flex;
      justify-content: flex-end;
    }
  }
}
</style>
