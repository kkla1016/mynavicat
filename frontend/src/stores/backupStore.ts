import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { BackupHistory, BackupRequest, RestoreRequest } from '../types/backup'
import {
  executeBackupApi,
  executeRestoreApi,
  executeRestoreFromFileApi,
  getBackupHistoryApi,
  deleteBackupHistoryApi
} from '../api/backup'

export const useBackupStore = defineStore('backup', () => {
  const historyList = ref<BackupHistory[]>([])
  const totalCount = ref(0)
  const loading = ref(false)
  const backingUp = ref(false)
  const restoring = ref(false)

  const fetchHistory = async (connectionId?: number, status?: string, page = 1, pageSize = 20) => {
    loading.value = true
    try {
      const res = await getBackupHistoryApi(connectionId, status, page, pageSize)
      if (res && res.success) {
        historyList.value = res.data.items
        totalCount.value = res.data.totalCount
      }
    } finally {
      loading.value = false
    }
  }

  const executeBackup = async (request: BackupRequest) => {
    backingUp.value = true
    try {
      const res = await executeBackupApi(request)
      return res
    } finally {
      backingUp.value = false
    }
  }

  const executeRestore = async (request: RestoreRequest) => {
    restoring.value = true
    try {
      const res = await executeRestoreApi(request)
      return res
    } finally {
      restoring.value = false
    }
  }

  const executeRestoreFromFile = async (formData: FormData) => {
    restoring.value = true
    try {
      const res = await executeRestoreFromFileApi(formData)
      return res
    } finally {
      restoring.value = false
    }
  }

  const deleteHistory = async (id: number) => {
    const res = await deleteBackupHistoryApi(id)
    if (res && res.success) {
      await fetchHistory()
    }
    return res
  }

  return {
    historyList,
    totalCount,
    loading,
    backingUp,
    restoring,
    fetchHistory,
    executeBackup,
    executeRestore,
    executeRestoreFromFile,
    deleteHistory
  }
})
