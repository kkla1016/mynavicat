<template>
  <div class="data-table-container">
    <el-table
      :data="data"
      style="width: 100%"
      border
      stripe
      v-loading="loading"
      height="100%"
    >
      <el-table-column
        v-for="col in columns"
        :key="col"
        :prop="col"
        :label="col"
        min-width="140"
        show-overflow-tooltip
      >
        <template #default="{ row }">
          <span v-if="row[col] === null" class="null-text">NULL</span>
          <span v-else>{{ row[col] }}</span>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-bar">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[20, 50, 100, 200]"
        layout="total, sizes, prev, pager, next, jumper"
        :total="total"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const props = defineProps<{
  data: Record<string, any>[]
  loading?: boolean
  total: number
}>()

const emit = defineEmits<{
  (e: 'page-change', payload: { page: number; pageSize: number }): void
}>()

const currentPage = ref(1)
const pageSize = ref(50)

const columns = computed(() => {
  if (!props.data || props.data.length === 0) return []
  return Object.keys(props.data[0])
})

const handleSizeChange = (newSize: number) => {
  pageSize.value = newSize
  emit('page-change', { page: currentPage.value, pageSize: newSize })
}

const handleCurrentChange = (newPage: number) => {
  currentPage.value = newPage
  emit('page-change', { page: newPage, pageSize: pageSize.value })
}
</script>

<style scoped lang="scss">
.data-table-container {
  display: flex;
  flex-direction: column;
  height: 100%;

  .null-text {
    color: #64748b;
    font-style: italic;
  }

  .pagination-bar {
    padding: 12px 16px;
    background-color: #1a2234;
    border-top: 1px solid #2e3a52;
    display: flex;
    justify-content: flex-end;
  }
}
</style>
