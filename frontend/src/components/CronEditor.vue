<template>
  <div class="cron-editor">
    <el-radio-group v-model="presetMode" @change="handlePresetChange" style="margin-bottom: 12px;">
      <el-radio value="daily">每天凌晨 00:00</el-radio>
      <el-radio value="hourly">每小時整點</el-radio>
      <el-radio value="weekly">每週日凌晨 00:00</el-radio>
      <el-radio value="custom">自訂 Cron</el-radio>
    </el-radio-group>

    <div v-if="presetMode === 'custom'" class="custom-cron-input">
      <el-input
        :model-value="modelValue"
        @update:model-value="$emit('update:modelValue', $event)"
        placeholder="例如: 0 2 * * *"
      >
        <template #prepend>Cron 表達式</template>
      </el-input>
      <div class="cron-hint">格式: [分] [時] [日] [月] [週] (例如 "0 2 * * *" 表示每天 02:00)</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
  modelValue: string
}>()

const emit = defineEmits(['update:modelValue'])

const presetMode = ref(props.modelValue === '0 0 * * *' ? 'daily' :
                      props.modelValue === '0 * * * *' ? 'hourly' :
                      props.modelValue === '0 0 * * 0' ? 'weekly' : 'custom')

const handlePresetChange = (mode: string) => {
  switch (mode) {
    case 'daily': emit('update:modelValue', '0 0 * * *'); break;
    case 'hourly': emit('update:modelValue', '0 * * * *'); break;
    case 'weekly': emit('update:modelValue', '0 0 * * 0'); break;
  }
}
</script>

<style scoped lang="scss">
.cron-editor {
  width: 100%;

  .cron-hint {
    font-size: 0.8rem;
    color: #64748b;
    margin-top: 4px;
  }
}
</style>
