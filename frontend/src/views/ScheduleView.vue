<template>
  <div class="page-container">
    <div class="toolbar">
      <el-button type="primary" :icon="Plus" @click="handleOpenAddModal">
        建立新排程
      </el-button>
      <el-button :icon="Refresh" @click="scheduleStore.fetchSchedules">
        重新整理
      </el-button>
    </div>

    <el-card class="table-card">
      <el-table :data="scheduleStore.schedules" v-loading="scheduleStore.loading" style="width: 100%">
        <el-table-column prop="description" label="排程描述 / 名稱" min-width="150" />
        <el-table-column prop="databaseName" label="目標資料庫" width="130" />
        <el-table-column prop="cronExpression" label="Cron 表達式" width="130">
          <template #default="{ row }">
            <el-tag type="info" size="small">{{ row.cronExpression }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="保留份數" width="100">
          <template #default="{ row }">
            {{ row.retainCount > 0 ? `${row.retainCount} 份` : '無限' }}
          </template>
        </el-table-column>
        <el-table-column label="啟用" width="90">
          <template #default="{ row }">
            <el-switch
              v-model="row.isEnabled"
              @change="handleToggle(row.id)"
            />
          </template>
        </el-table-column>
        <el-table-column label="上次執行" min-width="160">
          <template #default="{ row }">
            {{ formatDate(row.lastRunAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="success" plain @click="handleRunNow(row.id)">
              立即執行
            </el-button>
            <el-button size="small" type="primary" plain @click="handleOpenEditModal(row)">
              編輯
            </el-button>
            <el-popconfirm title="確定要刪除此排程嗎？" @confirm="handleDelete(row.id)">
              <template #reference>
                <el-button size="small" type="danger" plain>刪除</el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增 / 編輯排程對話框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '編輯定時排程' : '新增定時排程'"
      width="600px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="120px">
        <el-form-item label="排程描述" prop="description">
          <el-input v-model="form.description" placeholder="例如: 每日凌晨資料庫自動備份" />
        </el-form-item>

        <el-form-item label="目標連線" prop="connectionId">
          <ConnectionSelector v-model="form.connectionId" @update:model-value="handleConnChange" />
        </el-form-item>

        <el-form-item label="目標資料庫" prop="databaseName">
          <el-select v-model="form.databaseName" placeholder="請選擇資料庫" style="width: 100%" :loading="dbLoading">
            <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
          </el-select>
        </el-form-item>

        <el-form-item label="週期 Cron 設定" prop="cronExpression">
          <CronEditor v-model="cronExpressionValue" />
        </el-form-item>

        <el-form-item label="自動保留份數">
          <el-input-number v-model="form.retainCount" :min="0" :max="100" />
          <span style="margin-left: 8px; font-size: 0.85rem; color: #64748b;">(0 表示無限保留)</span>
        </el-form-item>

        <el-form-item label="檔案壓縮">
          <el-switch v-model="form.compressBackup" active-text="啟用壓縮 (.gz)" />
        </el-form-item>

        <el-form-item label="立即啟用">
          <el-switch v-model="form.isEnabled" />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">
            {{ isEdit ? '儲存變更' : '建立排程' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { Plus, Refresh } from '@element-plus/icons-vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import ConnectionSelector from '../components/ConnectionSelector.vue'
import CronEditor from '../components/CronEditor.vue'
import { useScheduleStore } from '../stores/scheduleStore'
import { getDatabasesApi } from '../api/browse'
import type { Schedule } from '../types/schedule'

const scheduleStore = useScheduleStore()

const dialogVisible = ref(false)
const isEdit = ref(false)
const submitting = ref(false)
const currentEditId = ref<number | null>(null)
const formRef = ref<FormInstance>()

const dbList = ref<string[]>([])
const dbLoading = ref(false)

const form = reactive<Partial<Schedule>>({
  description: '',
  connectionId: undefined,
  databaseName: '',
  cronExpression: '0 0 * * *',
  retainCount: 10,
  compressBackup: true,
  compressionType: 'gz',
  isEnabled: true
})

const cronExpressionValue = computed({
  get: () => form.cronExpression || '0 0 * * *',
  set: (val: string) => { form.cronExpression = val }
})

const rules: FormRules = {
  connectionId: [{ required: true, message: '請選擇目標連線', trigger: 'change' }],
  databaseName: [{ required: true, message: '請選擇目標資料庫', trigger: 'change' }],
  cronExpression: [{ required: true, message: '請設定 Cron 表達式', trigger: 'blur' }]
}

onMounted(() => {
  scheduleStore.fetchSchedules()
})

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString()
}

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

const handleOpenAddModal = () => {
  isEdit.value = false
  currentEditId.value = null
  Object.assign(form, {
    description: '',
    connectionId: undefined,
    databaseName: '',
    cronExpression: '0 0 * * *',
    retainCount: 10,
    compressBackup: true,
    compressionType: 'gz',
    isEnabled: true
  })
  dialogVisible.value = true
}

const handleOpenEditModal = async (row: Schedule) => {
  isEdit.value = true
  currentEditId.value = row.id
  Object.assign(form, {
    description: row.description,
    connectionId: row.connectionId,
    databaseName: row.databaseName,
    cronExpression: row.cronExpression,
    retainCount: row.retainCount,
    compressBackup: row.compressBackup,
    compressionType: row.compressionType,
    isEnabled: row.isEnabled
  })
  if (row.connectionId) {
    await handleConnChange(row.connectionId)
    form.databaseName = row.databaseName
  }
  dialogVisible.value = true
}

const handleToggle = async (id: number) => {
  await scheduleStore.toggleSchedule(id)
  ElMessage.success('排程啟用狀態已更新')
}

const handleRunNow = async (id: number) => {
  const res = await scheduleStore.runNowSchedule(id)
  if (res && res.success) {
    ElMessage.success('已觸發立即執行排程任務')
  }
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    submitting.value = true
    try {
      if (isEdit.value && currentEditId.value) {
        const res = await scheduleStore.updateSchedule(currentEditId.value, form)
        if (res && res.success) {
          ElMessage.success('排程設定已更新')
          dialogVisible.value = false
        }
      } else {
        const res = await scheduleStore.createSchedule(form)
        if (res && res.success) {
          ElMessage.success('排程已建立並註冊定時任務')
          dialogVisible.value = false
        }
      }
    } finally {
      submitting.value = false
    }
  })
}

const handleDelete = async (id: number) => {
  const res = await scheduleStore.deleteSchedule(id)
  if (res && res.success) {
    ElMessage.success('排程已刪除')
  }
}
</script>

<style scoped lang="scss">
.page-container {
  padding: 24px;

  .toolbar {
    margin-bottom: 16px;
    display: flex;
    gap: 12px;
  }

  .table-card {
    background-color: #1d263b;
    border-color: #2e3a52;
  }

  .dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }
}
</style>
