<template>
  <div class="browse-page">
    <div class="tree-sidebar">
      <DatabaseTree @select-table="handleSelectTable" />
    </div>

    <div class="main-content">
      <div v-if="selectedTarget" class="table-view">
        <div class="table-header">
          <div class="title">
            <el-tag size="large" type="primary">{{ selectedTarget.database }}</el-tag>
            <span class="table-name">{{ selectedTarget.table }}</span>
          </div>
        </div>

        <el-tabs v-model="activeTab" class="content-tabs">
          <el-tab-pane label="資料預覽" name="data">
            <DataTable
              :data="tableData"
              :loading="dataLoading"
              :total="totalCount"
              @page-change="handlePageChange"
            />
          </el-tab-pane>

          <el-tab-pane label="欄位結構 (Schema)" name="schema">
            <el-table :data="schema?.columns || []" v-loading="schemaLoading" border stripe style="width: 100%">
              <el-table-column prop="name" label="欄位名稱" min-width="150" />
              <el-table-column prop="dataType" label="資料型別" width="130" />
              <el-table-column label="Primary Key" width="120">
                <template #default="{ row }">
                  <el-tag v-if="row.isPrimaryKey" type="warning" size="small">PK</el-tag>
                </template>
              </el-table-column>
              <el-table-column label="Allow Null" width="110">
                <template #default="{ row }">
                  <span :style="{ color: row.isNullable ? '#10b981' : '#ef4444' }">
                    {{ row.isNullable ? 'YES' : 'NO' }}
                  </span>
                </template>
              </el-table-column>
              <el-table-column prop="defaultValue" label="預設值" width="140" />
              <el-table-column prop="comment" label="備註描述" min-width="180" />
            </el-table>
          </el-tab-pane>
        </el-tabs>
      </div>

      <div v-else class="empty-placeholder">
        <el-empty description="請在左側點選資料表以檢視資料與結構" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import DatabaseTree from '../components/DatabaseTree.vue'
import DataTable from '../components/DataTable.vue'
import { getTableSchemaApi, getTableDataApi } from '../api/browse'
import type { TableSchema } from '../types/browse'

interface SelectedTableTarget {
  connectionId: number
  database: string
  table: string
}

const selectedTarget = ref<SelectedTableTarget | null>(null)
const activeTab = ref('data')

const tableData = ref<Record<string, any>[]>([])
const totalCount = ref(0)
const dataLoading = ref(false)

const schema = ref<TableSchema | null>(null)
const schemaLoading = ref(false)

const handleSelectTable = async (payload: SelectedTableTarget) => {
  selectedTarget.value = payload
  await fetchTableData(1, 50)
  await fetchTableSchema()
}

const fetchTableData = async (page = 1, pageSize = 50) => {
  if (!selectedTarget.value) return
  dataLoading.value = true
  try {
    const res = await getTableDataApi(
      selectedTarget.value.connectionId,
      selectedTarget.value.database,
      selectedTarget.value.table,
      page,
      pageSize
    )
    if (res && res.success) {
      tableData.value = res.data.items
      totalCount.value = res.data.totalCount
    }
  } finally {
    dataLoading.value = false
  }
}

const fetchTableSchema = async () => {
  if (!selectedTarget.value) return
  schemaLoading.value = true
  try {
    const res = await getTableSchemaApi(
      selectedTarget.value.connectionId,
      selectedTarget.value.database,
      selectedTarget.value.table
    )
    if (res && res.success) {
      schema.value = res.data
    }
  } finally {
    schemaLoading.value = false
  }
}

const handlePageChange = ({ page, pageSize }: { page: number; pageSize: number }) => {
  fetchTableData(page, pageSize)
}
</script>

<style scoped lang="scss">
.browse-page {
  display: flex;
  width: 100%;
  height: 100%;

  .tree-sidebar {
    width: 280px;
    height: 100%;
  }

  .main-content {
    flex: 1;
    height: 100%;
    overflow: hidden;

    .table-view {
      display: flex;
      flex-direction: column;
      height: 100%;
      padding: 16px;

      .table-header {
        margin-bottom: 12px;

        .title {
          display: flex;
          align-items: center;
          gap: 12px;

          .table-name {
            font-size: 1.2rem;
            font-weight: 600;
            color: #e2e8f0;
          }
        }
      }

      .content-tabs {
        flex: 1;
        display: flex;
        flex-direction: column;
        overflow: hidden;

        :deep(.el-tabs__content) {
          flex: 1;
          overflow-y: auto;
        }

        :deep(.el-tab-pane) {
          height: 100%;
        }
      }
    }

    .empty-placeholder {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100%;
    }
  }
}
</style>
