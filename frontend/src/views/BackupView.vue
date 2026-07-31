<template>
  <div class="page-container" v-loading="backupStore.backingUp" element-loading-text="資料庫備份中，請稍候...">
    <el-card class="form-card">
      <template #header>
        <div class="card-header">
          <span>建立資料庫備份</span>
        </div>
      </template>

      <el-form label-width="120px" class="backup-form">
        <el-form-item label="目標資料庫連線">
          <ConnectionSelector v-model="selectedConnId" @update:model-value="handleConnChange" />
        </el-form-item>

        <el-form-item label="資料庫名稱">
          <el-select
            v-model="selectedDb"
            placeholder="請選擇要備份的資料庫"
            style="width: 100%"
            :disabled="!selectedConnId"
            :loading="dbLoading"
            @change="handleDbChange"
          >
            <el-option v-for="db in dbList" :key="db" :label="db" :value="db" />
          </el-select>
        </el-form-item>

        <el-form-item label="備份範圍">
          <el-radio-group v-model="backupScope">
            <el-radio value="all">完整備份 (所有資料表)</el-radio>
            <el-radio value="custom">指定資料表</el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item v-if="backupScope === 'custom'" label="選擇資料表">
          <el-select
            v-model="selectedTables"
            multiple
            collapse-tags
            collapse-tags-tooltip
            placeholder="請選擇資料表"
            style="width: 100%"
            :loading="tableLoading"
          >
            <el-option v-for="t in tableList" :key="t.name" :label="t.name" :value="t.name" />
          </el-select>
        </el-form-item>

        <el-form-item label="大型庫分片">
          <el-switch v-model="enableChunking" active-text="啟用分片備份 (Chunking)" />
        </el-form-item>

        <el-form-item v-if="enableChunking" label="單一分片上限">
          <el-input-number v-model="chunkSizeMb" :min="10" :max="10240" />
          <span style="margin-left: 8px; font-size: 0.85rem; color: #64748b;">MB (預設 100 MB)</span>
        </el-form-item>

        <el-form-item label="檔案壓縮">
          <el-switch v-model="compress" active-text="啟用壓縮 (.gz / .zip)" />
        </el-form-item>

        <el-form-item v-if="compress" label="壓縮格式">
          <el-radio-group v-model="compressionType">
            <el-radio value="gz">GZIP (.gz)</el-radio>
            <el-radio value="zip">ZIP (.zip)</el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            size="large"
            :disabled="!selectedConnId || !selectedDb"
            :loading="backupStore.backingUp"
            @click="handleStartBackup"
          >
            <el-icon style="margin-right: 6px;"><Download /></el-icon>
            立即開始備份
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Download } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import ConnectionSelector from '../components/ConnectionSelector.vue'
import { useBackupStore } from '../stores/backupStore'
import { getDatabasesApi, getTablesApi } from '../api/browse'
import type { TableInfo } from '../types/browse'

const backupStore = useBackupStore()

const selectedConnId = ref<number | null>(null)
const selectedDb = ref('')
const dbList = ref<string[]>([])
const dbLoading = ref(false)

const backupScope = ref<'all' | 'custom'>('all')
const selectedTables = ref<string[]>([])
const tableList = ref<TableInfo[]>([])
const tableLoading = ref(false)

const enableChunking = ref(false)
const chunkSizeMb = ref(100)

const compress = ref(true)
const compressionType = ref<'gz' | 'zip'>('gz')

const handleConnChange = async (connId: number) => {
  selectedDb.value = ''
  dbList.value = []
  selectedTables.value = []
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
  selectedTables.value = []
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

const handleStartBackup = async () => {
  if (!selectedConnId.value || !selectedDb.value) return

  try {
    const res = await backupStore.executeBackup({
      connectionId: selectedConnId.value,
      databaseName: selectedDb.value,
      tables: backupScope.value === 'custom' ? selectedTables.value : undefined,
      compress: compress.value,
      compressionType: compressionType.value
    })

    if (res && res.success) {
      ElMessage.success(`資料庫備份成功！檔案大小: ${(res.data.fileSize / 1024 / 1024).toFixed(2)} MB`)
    } else {
      ElMessage.error('備份失敗: ' + (res?.message || '未知錯誤'))
    }
  } catch (err: any) {
    ElMessage.error('備份發生例外: ' + (err.message || '連線逾時'))
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
}
</style>
