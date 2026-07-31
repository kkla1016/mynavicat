import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useConnectionStore = defineStore('connection', () => {
  const connections = ref<any[]>([])
  const loading = ref(false)

  const connectionList = computed(() => connections.value)

  const fetchConnections = async () => {
    loading.value = true
    try {
      if (window.electronAPI) {
        connections.value = await window.electronAPI.getConnections()
      }
    } finally {
      loading.value = false
    }
  }

  const saveConnection = async (conn: any) => {
    if (window.electronAPI) {
      const result = await window.electronAPI.saveConnection(conn)
      await fetchConnections()
      return { success: true, data: result }
    }
    return { success: false, message: 'Electron API 不可用' }
  }

  const createConnection = saveConnection
  const updateConnection = async (_id: number, conn: any) => saveConnection(conn)

  const deleteConnection = async (id: number) => {
    if (window.electronAPI) {
      const ok = await window.electronAPI.deleteConnection(id)
      if (ok) await fetchConnections()
      return { success: ok }
    }
    return { success: false }
  }

  const testConnection = async (conn: any) => {
    if (window.electronAPI) {
      const ok = await window.electronAPI.testConnection(conn)
      return { success: ok, message: ok ? '連線成功！' : '無法建立連線' }
    }
    return { success: false, message: 'Electron API 不可用' }
  }

  const testConnectionDto = testConnection

  return {
    connections,
    connectionList,
    loading,
    fetchConnections,
    saveConnection,
    createConnection,
    updateConnection,
    deleteConnection,
    testConnection,
    testConnectionDto
  }
})
