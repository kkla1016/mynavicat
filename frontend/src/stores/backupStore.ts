import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useBackupStore = defineStore('backup', () => {
  const backingUp = ref(false)
  const restoring = ref(false)
  const histories = ref<any[]>([])

  const loading = computed(() => backingUp.value || restoring.value)
  const historyList = computed(() => histories.value)
  const totalCount = computed(() => histories.value.length)

  const executeBackup = async (request: any) => {
    backingUp.value = true
    try {
      if (window.electronAPI) {
        const result = await window.electronAPI.executeBackup(request)
        return { success: result?.Status === 'Success', data: result, message: result?.ErrorMessage || '備份完成' }
      }
    } finally {
      backingUp.value = false
    }
  }

  const fetchHistories = async (connectionId?: number, status?: string, _page?: number, _pageSize?: number) => {
    if (window.electronAPI) {
      histories.value = await window.electronAPI.getBackupHistories(connectionId, status)
    }
  }

  const fetchHistory = fetchHistories

  const deleteHistory = async (id: number) => {
    if (window.electronAPI) {
      const ok = await window.electronAPI.deleteBackupHistory(id)
      if (ok) await fetchHistories()
      return { success: ok }
    }
    return { success: false }
  }

  const executeRestore = async (req: any) => {
    restoring.value = true
    try {
      if (window.electronAPI) {
        const ok = await window.electronAPI.executeRestore(req)
        return { success: ok, message: ok ? '還原完成！' : '還原失敗' }
      }
    } finally {
      restoring.value = false
    }
    return { success: false, message: 'Electron API 不可用' }
  }

  const executeRestoreFromFile = async (connId: number, database: string, file: any) => {
    restoring.value = true
    try {
      if (window.electronAPI) {
        const ok = await window.electronAPI.executeRestore({ targetConnectionId: connId, targetDatabaseName: database, filePath: file?.path })
        return { success: ok, message: ok ? '還原完成！' : '還原失敗' }
      }
    } finally {
      restoring.value = false
    }
    return { success: false, message: 'Electron API 不可用' }
  }

  return {
    backingUp,
    restoring,
    loading,
    histories,
    historyList,
    totalCount,
    executeBackup,
    fetchHistories,
    fetchHistory,
    deleteHistory,
    executeRestore,
    executeRestoreFromFile
  }
})
