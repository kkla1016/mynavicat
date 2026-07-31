import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    redirect: '/connections'
  },
  {
    path: '/connections',
    name: 'Connections',
    component: () => import('../views/ConnectionsView.vue')
  },
  {
    path: '/backup',
    name: 'Backup',
    component: () => import('../views/BackupView.vue')
  },
  {
    path: '/restore',
    name: 'Restore',
    component: () => import('../views/RestoreView.vue')
  },
  {
    path: '/schedules',
    name: 'Schedules',
    component: () => import('../views/ScheduleView.vue')
  },
  {
    path: '/history',
    name: 'History',
    component: () => import('../views/HistoryView.vue')
  },
  {
    path: '/browse',
    name: 'Browse',
    component: () => import('../views/BrowseView.vue')
  },
  {
    path: '/export-import',
    name: 'ExportImport',
    component: () => import('../views/ExportImportView.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
