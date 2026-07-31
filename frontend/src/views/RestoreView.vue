<template>
  <div class="page-container" v-loading="backupStore.restoring" element-loading-text="資料庫還原中，請稍候...">
    <el-card class="form-card">
      <template #header>
        <div class="card-header">
          <span>資料庫還原</span>
        </div>
      </template>

      <el-tabs v-model="restoreMode">
        <el-tab-pane label="從歷史記錄還原" name="history">
          <el-form label-width="120px" class="restore-form">
            <el-form-item label="選擇歷史備份">
              <el-select
                v-model="selectedHistoryId"
                placeholder="請選擇備份記錄"
                style="width: 100%"
                filterable
              >
                <el-option
                  v-for="item in historyOptions"
                  :key="item.id"
                  :label="`${item.databaseName} - ${item.startedAt} (${item.backupType})`"
                  :value="item.id"
                />
              </el-select>
            </el-form-item>

            <el-form-item label="目標連線">
              <ConnectionSelector v-model="targetConnId" />
            </el-form-item>

            <el-form-item label="目標資料庫名稱">
              <el-input v-model="targetDbName" placeholder="例如: target_db" />
            </el-form-item>

            <el-form-item>
              <el-button
                type="warning"
                size="large"
                :disabled="!selectedHistoryId || !targetConnId || !targetDbName"
                :loading="backupStore.restoring"
                @click="handleRestoreFromHistory"
              >
                <el-icon style="margin-right: 6px;"><Upload /></el-icon>
                開始從歷史記錄還原
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="上傳檔案還原" name="file">
          <el-form label-width="120px" class="restore-form">
            <el-form-item label="上傳備份檔">
              <el-upload
                ref="uploadRef"
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

            <el-form-item label="目標連線">
              <ConnectionSelector v-model="targetConnId" />
            </el-form-item>

            <el-form-item label="目標資料庫名稱">
              <el-input v-model="targetDbName" placeholder="例如: target_db" />
            </el-form-item>

            <el-form-item>
              <el-button
                type="warning"
                size="large"
                :disabled="!uploadFile || !targetConnId || !targetDbName"
                :loading="backupStore.restoring"
                @click="handleRestoreFromFile"
              >
                <el-icon style="margin-right: 6px;"><Upload /></el-icon>
                上傳並還原資料庫
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { Upload } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import ConnectionSelector from '../components/ConnectionSelector.vue'
import { useBackupStore } from '../stores/backupStore'

const backupStore = useBackupStore()

const restoreMode = ref('history')
const selectedHistoryId = ref<number | null>(null)
const targetConnId = ref<number | null>(null)
const targetDbName = ref('')
const uploadFile = ref<File | null>(null)

const historyOptions = computed(() => backupStore.historyList.filter(h => h.status === 'Success'))

onMounted(() => {
  backupStore.fetchHistory()
})

const handleFileChange = (file: any) => {
  uploadFile.value = file.raw
}

const handleRestoreFromHistory = async () => {
  if (!selectedHistoryId.value || !targetConnId.value || !targetDbName.value) return

  try {
    const res = await backupStore.executeRestore({
      backupHistoryId: selectedHistoryId.value,
      targetConnectionId: targetConnId.value,
      targetDatabaseName: targetDbName.value
    })

    if (res && res.success) {
      ElMessage.success('資料庫已成功還原！')
    } else {
      ElMessage.error('還原失敗: ' + (res?.message || '未知錯誤'))
    }
  } catch (err: any) {
    ElMessage.error('還原發生例外: ' + (err.message || '未知錯誤'))
  }
}

const handleRestoreFromFile = async () => {
  if (!uploadFile.value || !targetConnId.value || !targetDbName.value) return

  const formData = new FormData()
  formData.append('targetConnectionId', targetConnId.value.toString())
  formData.append('targetDatabaseName', targetDbName.value)
  formData.append('file', uploadFile.value)

  try {
    const res = await backupStore.executeRestoreFromFile(formData)
    if (res && res.success) {
      ElMessage.success('上傳還原資料庫成功！')
    } else {
      ElMessage.error('還原失敗: ' + (res?.message || '未知錯誤'))
    }
  } catch (err: any) {
    ElMessage.error('上傳還原發生例外: ' + (err.message || '未知錯誤'))
  }
}
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
