<template>
  <div class="page-container" v-loading="backupStore.restoring" element-loading-text="資料庫還原中，請稍候...">
    <el-card class="form-card">
      <template #header>
        <div class="card-header">
          <span>執行資料庫還原</span>
        </div>
      </template>

      <el-tabs v-model="activeTab">
        <el-tab-pane label="從歷史紀錄還原" name="history">
          <el-form label-width="120px" class="restore-form">
            <el-form-item label="目標資料庫連線">
              <ConnectionSelector v-model="selectedConnId" @update:model-value="handleConnChange" />
            </el-form-item>

            <el-form-item label="目標資料庫名稱">
              <el-select v-model="selectedDb" placeholder="請選擇目標資料庫" style="width: 100%" :loading="dbLoading">
                <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
              </el-select>
            </el-form-item>

            <el-form-item label="選擇備份檔">
              <el-select v-model="selectedHistoryId" placeholder="請選擇歷史備份紀錄" style="width: 100%">
                <el-option
                  v-for="h in backupStore.histories"
                  :key="h.Id"
                  :label="`${h.DatabaseName} (${h.StartedAt}) - ${(h.FileSize/1024/1024).toFixed(2)} MB`"
                  :value="h.Id"
                />
              </el-select>
            </el-form-item>

            <el-form-item>
              <el-button
                type="warning"
                size="large"
                :disabled="!selectedConnId || !selectedDb || !selectedHistoryId"
                :loading="backupStore.restoring"
                @click="handleRestoreFromHistory"
              >
                <el-icon style="margin-right: 6px;"><Upload /></el-icon>
                開始從歷史還原
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="上傳外部檔案還原" name="file">
          <el-form label-width="120px" class="restore-form">
            <el-form-item label="目標資料庫連線">
              <ConnectionSelector v-model="selectedConnId" @update:model-value="handleConnChange" />
            </el-form-item>

            <el-form-item label="目標資料庫名稱">
              <el-select v-model="selectedDb" placeholder="請選擇目標資料庫" style="width: 100%" :loading="dbLoading">
                <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
              </el-select>
            </el-form-item>

            <el-form-item label="上傳備份檔">
              <el-upload
                action=""
                :auto-upload="false"
                :limit="1"
                :on-change="handleFileChange"
                accept=".sql,.gz,.zip"
              >
                <template #trigger>
                  <el-button type="primary">選擇檔案 (.sql / .gz / .zip)</el-button>
                </template>
              </el-upload>
            </el-form-item>

            <el-form-item>
              <el-button
                type="danger"
                size="large"
                :disabled="!selectedConnId || !selectedDb || !uploadFile"
                :loading="backupStore.restoring"
                @click="handleRestoreFromFile"
              >
                <el-icon style="margin-right: 6px;"><Upload /></el-icon>
                開始上傳檔還原
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Upload } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import ConnectionSelector from '../components/ConnectionSelector.vue'
import { useBackupStore } from '../stores/backupStore'
import { getDatabasesApi } from '../api/browse'

const backupStore = useBackupStore()
const activeTab = ref('history')

const selectedConnId = ref<number | null>(null)
const selectedDb = ref('')
const dbList = ref<string[]>([])
const dbLoading = ref(false)

const selectedHistoryId = ref<number | null>(null)
const uploadFile = ref<any>(null)

const handleConnChange = async (connId: number) => {
  selectedDb.value = ''
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

const handleFileChange = (file: any) => {
  uploadFile.value = file.raw
}

const handleRestoreFromHistory = async () => {
  if (!selectedConnId.value || !selectedDb.value || !selectedHistoryId.value) return

  try {
    await ElMessageBox.confirm('還原操作將覆寫目標資料庫中的現有資料，確定繼續嗎？', '警告', {
      type: 'warning'
    })

    const res = await backupStore.executeRestore({
      targetConnectionId: selectedConnId.value,
      targetDatabaseName: selectedDb.value,
      backupHistoryId: selectedHistoryId.value
    })

    if (res && res.success) {
      ElMessage.success('資料庫還原成功！')
    } else {
      ElMessage.error('還原失敗: ' + (res?.message || '未知錯誤'))
    }
  } catch {}
}

const handleRestoreFromFile = async () => {
  if (!selectedConnId.value || !selectedDb.value || !uploadFile.value) return

  try {
    await ElMessageBox.confirm('還原操作將覆寫目標資料庫中的現有資料，確定繼續嗎？', '警告', {
      type: 'warning'
    })

    const res = await backupStore.executeRestoreFromFile(selectedConnId.value, selectedDb.value, uploadFile.value)

    if (res && res.success) {
      ElMessage.success('外部檔案還原成功！')
    } else {
      ElMessage.error('還原失敗: ' + (res?.message || '未知錯誤'))
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
  max-width: 800px;

  .form-card {
    background-color: #1d263b;
    border-color: #2e3a52;

    .card-header {
      font-size: 1.1rem;
      font-weight: 600;
    }
  }

  .restore-form {
    margin-top: 16px;
  }
}
</style>
