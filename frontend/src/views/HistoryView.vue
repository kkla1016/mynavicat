<template>
  <div class="page-container">
    <el-card class="history-card">
      <template #header>
        <div class="card-header">
          <span>備份歷史記錄</span>
          <el-button type="primary" size="small" @click="handleRefresh">重新整理</el-button>
        </div>
      </template>

      <el-table
        :data="backupStore.histories"
        style="width: 100%"
        v-loading="backupStore.loading"
        stripe
      >
        <el-table-column prop="Id" label="ID" width="70" />
        <el-table-column prop="ConnectionName" label="目標連線" width="140" />
        <el-table-column prop="DatabaseName" label="資料庫" width="140" />
        <el-table-column prop="BackupType" label="類型" width="100">
          <template #default="scope">
            <el-tag :type="scope.row.BackupType === 'Chunked' ? 'warning' : 'info'">
              {{ scope.row.BackupType }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="大小" width="110">
          <template #default="scope">
            {{ (scope.row.FileSize / 1024 / 1024).toFixed(2) }} MB
          </template>
        </el-table-column>
        <el-table-column label="狀態" width="100">
          <template #default="scope">
            <el-tag :type="scope.row.Status === 'Success' ? 'success' : 'danger'">
              {{ scope.row.Status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="StartedAt" label="開始時間" width="180" />
        <el-table-column label="操作" min-width="120">
          <template #default="scope">
            <el-button
              type="danger"
              size="small"
              @click="handleDelete(scope.row.Id)"
            >
              刪除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useBackupStore } from '../stores/backupStore'

const backupStore = useBackupStore()

const handleRefresh = () => {
  backupStore.fetchHistories()
}

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm('確定要刪除此筆歷史紀錄與對應檔案嗎？', '刪除確認', {
      type: 'warning'
    })
    const res = await backupStore.deleteHistory(id)
    if (res && res.success) {
      ElMessage.success('紀錄與備份檔已刪除')
    }
  } catch {}
}

onMounted(() => {
  backupStore.fetchHistories()
})
</script>

<style scoped lang="scss">
.page-container {
  padding: 24px;

  .history-card {
    background-color: #1d263b;
    border-color: #2e3a52;

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 1.1rem;
      font-weight: 600;
    }
  }
}
</style>
