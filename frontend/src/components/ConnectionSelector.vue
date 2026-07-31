<template>
  <el-select
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    placeholder="請選擇資料庫連線"
    style="width: 100%"
    filterable
    :loading="connectionStore.loading"
  >
    <el-option
      v-for="item in connectionStore.connections"
      :key="item.id"
      :label="`${item.name} (${item.dbType} - ${item.host}:${item.port})`"
      :value="item.id"
    >
      <div class="option-item">
        <span>{{ item.name }}</span>
        <el-tag size="small" type="info">{{ item.dbType }}</el-tag>
      </div>
    </el-option>
  </el-select>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useConnectionStore } from '../stores/connectionStore'

defineProps<{
  modelValue?: number | null
}>()

defineEmits(['update:modelValue'])

const connectionStore = useConnectionStore()

onMounted(() => {
  if (connectionStore.connections.length === 0) {
    connectionStore.fetchConnections()
  }
})
</script>

<style scoped>
.option-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}
</style>
