<template>
  <aside class="sidebar">
    <div class="logo">
      <el-icon class="logo-icon"><Connection /></el-icon>
      <span class="logo-text">MyNavicat</span>
    </div>
    <el-menu
      :default-active="activeRoute"
      class="sidebar-menu"
      router
      background-color="#1a2234"
      text-color="#94a3b8"
      active-text-color="#3b82f6"
    >
      <el-menu-item index="/connections">
        <el-icon><Link /></el-icon>
        <span>連線管理</span>
      </el-menu-item>
      <el-menu-item index="/browse">
        <el-icon><FolderOpened /></el-icon>
        <span>資料表瀏覽</span>
      </el-menu-item>
      <el-menu-item index="/backup">
        <el-icon><Download /></el-icon>
        <span>資料庫備份</span>
      </el-menu-item>
      <el-menu-item index="/restore">
        <el-icon><Upload /></el-icon>
        <span>資料庫還原</span>
      </el-menu-item>
      <el-menu-item index="/schedules">
        <el-icon><Timer /></el-icon>
        <span>排程管理</span>
      </el-menu-item>
      <el-menu-item index="/history">
        <el-icon><Clock /></el-icon>
        <span>備份歷史</span>
      </el-menu-item>
      <el-menu-item index="/export-import">
        <el-icon><DocumentCopy /></el-icon>
        <span>資料匯出/匯入</span>
      </el-menu-item>
    </el-menu>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import {
  Connection,
  Link,
  FolderOpened,
  Download,
  Upload,
  Timer,
  Clock,
  DocumentCopy
} from '@element-plus/icons-vue'

const route = useRoute()
const activeRoute = computed(() => route.path)
</script>

<style scoped lang="scss">
.sidebar {
  width: 240px;
  height: 100%;
  background-color: #1a2234;
  border-right: 1px solid #2e3a52;
  display: flex;
  flex-direction: column;

  .logo {
    height: 56px;
    display: flex;
    align-items: center;
    padding: 0 20px;
    border-bottom: 1px solid #2e3a52;
    color: #e2e8f0;
    font-weight: 600;
    font-size: 1.1rem;

    .logo-icon {
      font-size: 1.4rem;
      color: #3b82f6;
      margin-right: 10px;
    }
  }

  .sidebar-menu {
    border-right: none;
    flex: 1;
  }
}
</style>
