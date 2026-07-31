<template>
  <div class="page-container">
    <el-card class="schedule-card">
      <template #header>
        <div class="card-header">
          <span>排程自動備份管理</span>
          <el-button type="primary" @click="handleOpenAddDialog">
            <el-icon style="margin-right: 4px;"><Plus /></el-icon>
            新增自動排程
          </el-button>
        </div>
      </template>

      <el-table :data="scheduleStore.schedules" style="width: 100%" v-loading="scheduleStore.loading" stripe>
        <el-table-column prop="Id" label="ID" width="70" />
        <el-table-column prop="ConnectionName" label="連線" width="140" />
        <el-table-column prop="DatabaseName" label="資料庫" width="140" />
        <el-table-column prop="CronExpression" label="Cron 表達式" width="160" />
        <el-table-column label="保留份數" width="100">
          <template #default="scope">
            {{ scope.row.RetainCount }} 份
          </template>
        </el-table-column>
        <el-table-column label="狀態" width="100">
          <template #default="scope">
            <el-switch
              v-model="scope.row.IsEnabled"
              :active-value="1"
              :inactive-value="0"
              @change="(val: any) => handleToggle(scope.row.Id, val)"
            />
          </template>
        </el-table-column>
        <el-table-column label="操作" min-width="180">
          <template #default="scope">
            <el-button type="success" size="small" @click="handleRunNow(scope.row.Id)">立即執行</el-button>
            <el-button type="primary" size="small" @click="handleEdit(scope.row)">編輯</el-button>
            <el-button type="danger" size="small" @click="handleDelete(scope.row.Id)">刪除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="editingId ? '編輯自動備份排程' : '新增自動備份排程'" width="600px">
      <el-form label-width="120px">
        <el-form-item label="目標連線">
          <ConnectionSelector v-model="form.connectionId" @update:model-value="handleConnChange" />
        </el-form-item>

        <el-form-item label="資料庫名稱">
          <el-select v-model="form.databaseName" placeholder="請選擇資料庫" style="width: 100%" :loading="dbLoading">
            <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
          </el-select>
        </el-form-item>

        <el-form-item label="Cron 時間設定">
          <CronEditor v-model="form.cronExpression" />
        </el-form-item>

        <el-form-item label="保留歷史備份">
          <el-input-number v-model="form.retainCount" :min="1" :max="100" />
          <span style="margin-left: 8px; font-size: 0.85rem; color: #64748b;">超過的舊檔案將自動刪除</span>
        </el-form-item>

        <el-form-item label="啟用壓縮">
          <el-switch v-model="form.compress" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">儲存排程</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { Plus } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import ConnectionSelector from '../components/ConnectionSelector.vue'
import CronEditor from '../components/CronEditor.vue'
import { useScheduleStore } from '../stores/scheduleStore'
import { getDatabasesApi } from '../api/browse'

const scheduleStore = useScheduleStore()

const dialogVisible = ref(false)
const editingId = ref<number | null>(null)

const dbList = ref<string[]>([])
const dbLoading = ref(false)

const form = reactive({
  connectionId: null as number | null,
  databaseName: '',
  cronExpression: '0 2 * * *',
  retainCount: 7,
  compress: true,
  compressionType: 'gz'
})

const handleConnChange = async (connId: number) => {
  form.databaseName = ''
  dbList.value = []

  if (!connId) return
  dbLoading.value = true
  try {
    const res = await getDatabasesApi(connId)
    if (res && res.success) {
      dbList.value = res.data
    }
  } finally {
    dbLoading.value = false
  }
}

const handleOpenAddDialog = () => {
  editingId.value = null
  form.connectionId = null
  form.databaseName = ''
  form.cronExpression = '0 2 * * *'
  form.retainCount = 7
  form.compress = true
  dialogVisible.value = true
}

const handleEdit = (row: any) => {
  editingId.value = row.Id
  form.connectionId = row.ConnectionId
  form.databaseName = row.DatabaseName
  form.cronExpression = row.CronExpression
  form.retainCount = row.RetainCount
  form.compress = row.Compress === 1
  dialogVisible.value = true
}

const handleToggle = async (id: number, val: any) => {
  const isEnabled = val === 1 || val === true
  const res = await scheduleStore.toggleSchedule(id, isEnabled)
  if (res && res.success) {
    ElMessage.success('排程狀態已更新')
  }
}

const handleRunNow = async (id: number) => {
  const res = await scheduleStore.runScheduleNow(id)
  if (res && res.success) {
    ElMessage.success('排程備份觸發成功')
  } else {
    ElMessage.error('觸發失敗')
  }
}

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm('確定要刪除此排程嗎？', '刪除確認', { type: 'warning' })
    const res = await scheduleStore.deleteSchedule(id)
    if (res && res.success) {
      ElMessage.success('排程已刪除')
    }
  } catch {}
}

const handleSave = async () => {
  if (!form.connectionId || !form.databaseName) return

  const payload = {
    Id: editingId.value,
    ConnectionId: form.connectionId,
    DatabaseName: form.databaseName,
    CronExpression: form.cronExpression,
    RetainCount: form.retainCount,
    Compress: form.compress,
    CompressionType: 'gz',
    IsEnabled: 1
  }

  const res = await scheduleStore.saveSchedule(payload)
  if (res && res.success) {
    ElMessage.success('排程已儲存')
    dialogVisible.value = false
  }
}

onMounted(() => {
  scheduleStore.fetchSchedules()
})
</script>

<style scoped lang="scss">
.page-container {
  padding: 24px;

  .schedule-card {
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
