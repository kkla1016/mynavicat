<template>
  <AppLayout />
</template>

<script setup lang="ts">
import AppLayout from './components/layout/AppLayout.vue'
import { useSignalR } from './composables/useSignalR'

// 初始化 SignalR 全域推播與桌面通知監聽
useSignalR()
</script>

<style>
/* App 根樣式 */
</style>
