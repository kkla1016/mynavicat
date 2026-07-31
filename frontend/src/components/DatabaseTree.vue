<template>
  <div class="db-tree-container">
    <div class="selector-box">
      <ConnectionSelector v-model="selectedConnId" @update:model-value="handleConnChange" />
    </div>

    <div class="tree-box" v-loading="loading">
      <el-tree
        v-if="treeData.length > 0"
        :data="treeData"
        :props="defaultProps"
        lazy
        :load="loadNode"
        node-key="id"
        highlight-current
        @node-click="handleNodeClick"
      >
        <template #default="{ node, data }">
          <span class="custom-tree-node">
            <el-icon class="node-icon">
              <Folder v-if="data.type === 'db'" />
              <Grid v-else-if="data.type === 'table'" />
            </el-icon>
            <span>{{ node.label }}</span>
          </span>
        </template>
      </el-tree>
      <div v-else class="empty-hint">
        請選擇資料庫連線
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Folder, Grid } from '@element-plus/icons-vue'
import ConnectionSelector from './ConnectionSelector.vue'
import { getDatabasesApi, getTablesApi } from '../api/browse'
import type { TableInfo } from '../types/browse'

interface TreeNode {
  id: string
  label: string
  type: 'db' | 'table'
  dbName?: string
  tableName?: string
  isLeaf?: boolean
}

const emit = defineEmits<{
  (e: 'select-table', payload: { connectionId: number; database: string; table: string }): void
}>()

const selectedConnId = ref<number | null>(null)
const treeData = ref<TreeNode[]>([])
const loading = ref(false)

const defaultProps = {
  label: 'label',
  children: 'children',
  isLeaf: 'isLeaf'
}

const handleConnChange = async (connId: number) => {
  if (!connId) return
  loading.value = true
  treeData.value = []
  try {
    const res = await getDatabasesApi(connId)
    if (res && res.success) {
      treeData.value = res.data.map((db: string) => ({
        id: `db-${db}`,
        label: db,
        type: 'db',
        dbName: db,
        isLeaf: false
      }))
    }
  } finally {
    loading.value = false
  }
}

const loadNode = async (node: any, resolve: Function) => {
  if (node.level === 0) return resolve(treeData.value)
  if (node.level === 1 && selectedConnId.value) {
    const dbName = node.data.dbName
    try {
      const res = await getTablesApi(selectedConnId.value, dbName)
      if (res && res.success) {
        const tableNodes: TreeNode[] = res.data.map((t: TableInfo) => ({
          id: `tbl-${dbName}-${t.name}`,
          label: t.name,
          type: 'table',
          dbName,
          tableName: t.name,
          isLeaf: true
        }))
        resolve(tableNodes)
      } else {
        resolve([])
      }
    } catch {
      resolve([])
    }
  } else {
    resolve([])
  }
}

const handleNodeClick = (data: TreeNode) => {
  if (data.type === 'table' && selectedConnId.value && data.dbName && data.tableName) {
    emit('select-table', {
      connectionId: selectedConnId.value,
      database: data.dbName,
      table: data.tableName
    })
  }
}
</script>

<style scoped lang="scss">
.db-tree-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  border-right: 1px solid #2e3a52;
  background-color: #1a2234;

  .selector-box {
    padding: 16px;
    border-bottom: 1px solid #2e3a52;
  }

  .tree-box {
    flex: 1;
    padding: 12px;
    overflow-y: auto;

    .empty-hint {
      color: #64748b;
      font-size: 0.9rem;
      text-align: center;
      margin-top: 40px;
    }
  }

  .custom-tree-node {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.9rem;

    .node-icon {
      color: #3b82f6;
    }
  }
}
</style>
