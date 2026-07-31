<template>
  <div class="page-container">
    <el-card class="form-card">
      <template #header>
        <div class="card-header">
          <span>資料庫資料 匯出 / 匯入</span>
        </div>
      </template>

      <el-tabs v-model="activeTab">
        <el-tab-pane label="資料表匯出" name="export">
          <el-form label-width="120px" class="operation-form">
            <el-form-item label="目標連線">
              <ConnectionSelector v-model="selectedConnId" @update:model-value="handleConnChange" />
            </el-form-item>

            <el-form-item label="資料庫名稱">
              <el-select v-model="selectedDb" placeholder="請選擇資料庫" style="width: 100%" :loading="dbLoading" @change="handleDbChange">
                <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
              </el-select>
            </el-form-item>

            <el-form-item label="目標資料表">
              <el-select v-model="selectedTable" placeholder="請選擇資料表" style="width: 100%" :loading="tableLoading">
                <el-option v-for="t in tableList" :key="t.name" :label="t.name" :value="t.name" />
              </el-select>
            </el-form-item>

            <el-form-item label="匯出格式">
              <el-radio-group v-model="exportFormat">
                <el-radio value="CSV">CSV 檔案 (.csv)</el-radio>
                <el-radio value="JSON">JSON 檔案 (.json)</el-radio>
                <el-radio value="SQL">SQL INSERT 命令 (.sql)</el-radio>
              </el-radio-group>
            </el-form-item>

            <el-form-item>
              <el-button
                type="primary"
                size="large"
                :disabled="!selectedConnId || !selectedDb || !selectedTable"
                :loading="exporting"
                @click="handleStartExport"
              >
                <el-icon style="margin-right: 6px;"><Download /></el-icon>
                選擇儲存路徑並匯出
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>

        <el-tab-pane label="資料表匯入" name="import">
          <el-form label-width="120px" class="operation-form">
            <el-form-item label="目標連線">
              <ConnectionSelector v-model="selectedConnId" @update:model-value="handleConnChange" />
            </el-form-item>

            <el-form-item label="資料庫名稱">
              <el-select v-model="selectedDb" placeholder="請選擇資料庫" style="width: 100%" :loading="dbLoading" @change="handleDbChange">
                <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
              </el-select>
            </el-form-item>

            <el-form-item label="目標資料表">
              <el-select v-model="selectedTable" placeholder="請選擇資料表" style="width: 100%" :loading="tableLoading">
                <el-option v-for="t in tableList" :key="t.name" :label="t.name" :value="t.name" />
              </el-select>
            </el-form-item>

            <el-form-item label="選擇匯入檔案">
              <el-button type="success" size="large" :disabled="!selectedConnId || !selectedDb || !selectedTable" :loading="importing" @click="handleStartImport">
                <el-icon style="margin-right: 6px;"><Upload /></el-icon>
                選擇 CSV / JSON 檔案並匯入
              </el-button>
            </el-form-item>
          </el-form>
        </el-tab-pane>
      </el-tabs>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Download, Upload } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import ConnectionSelector from '../components/ConnectionSelector.vue'
import { getDatabasesApi, getTablesApi } from '../api/browse'
import type { TableInfo } from '../types/browse'

const activeTab = ref('export')

const selectedConnId = ref<number | null>(null)
const selectedDb = ref('')
const dbList = ref<string[]>([])
const dbLoading = ref(false)

const selectedTable = ref('')
const tableList = ref<TableInfo[]>([])
const tableLoading = ref(false)

const exportFormat = ref('CSV')
const exporting = ref(false)
const importing = ref(false)

const handleConnChange = async (connId: number) => {
  selectedDb.value = ''
  dbList.value = []
  selectedTable.value = ''
  tableList.value = []

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

const handleDbChange = async (dbName: string) => {
  selectedTable.value = ''
  tableList.value = []

  if (!selectedConnId.value || !dbName) return
  tableLoading.value = true
  try {
    const res = await getTablesApi(selectedConnId.value, dbName)
    if (res && res.success) {
      tableList.value = res.data
    }
  } finally {
    tableLoading.value = false
  }
}

const handleStartExport = async () => {
  if (!selectedConnId.value || !selectedDb.value || !selectedTable.value) return
  exporting.value = true
  try {
    if (window.electronAPI) {
      const res = await window.electronAPI.exportTableData({
        connectionId: selectedConnId.value,
        databaseName: selectedDb.value,
        tableName: selectedTable.value,
        format: exportFormat.value
      })
      if (res && res.success) {
        ElMessage.success('檔案匯出成功！')
      }
    }
  } catch (err: any) {
    ElMessage.error('匯出失敗: ' + (err.message || '未知錯誤'))
  } finally {
    exporting.value = false
  }
}

const handleStartImport = async () => {
  if (!selectedConnId.value || !selectedDb.value || !selectedTable.value) return
  importing.value = true

  try {
    if (window.electronAPI) {
      const res = await window.electronAPI.importTableData({
        connectionId: selectedConnId.value,
        database: selectedDb.value,
        table: selectedTable.value
      })

      if (res && res.success) {
        ElMessage.success(`資料匯入完成！成功: ${res.data.successRows} 筆，失敗: ${res.data.failedRows} 筆`)
      }
    }
  } catch (err: any) {
    ElMessage.error('匯入失敗: ' + (err.message || '未知錯誤'))
  } finally {
    importing.value = false
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

  .operation-form {
    margin-top: 16px;
  }
}
</style>
