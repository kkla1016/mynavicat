import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Connection } from '../types/connection'
import {
  getConnectionsApi,
  createConnectionApi,
  updateConnectionApi,
  deleteConnectionApi,
  testConnectionApi,
  testConnectionDtoApi
} from '../api/connection'

export const useConnectionStore = defineStore('connection', () => {
  const connections = ref<Connection[]>([])
  const loading = ref(false)

  const fetchConnections = async () => {
    loading.value = true
    try {
      const res = await getConnectionsApi()
      if (res && res.success) {
        connections.value = res.data
      }
    } finally {
      loading.value = false
    }
  }

  const createConnection = async (data: Partial<Connection>) => {
    const res = await createConnectionApi(data)
    if (res && res.success) {
      await fetchConnections()
    }
    return res
  }

  const updateConnection = async (id: number, data: Partial<Connection>) => {
    const res = await updateConnectionApi(id, data)
    if (res && res.success) {
      await fetchConnections()
    }
    return res
  }

  const deleteConnection = async (id: number) => {
    const res = await deleteConnectionApi(id)
    if (res && res.success) {
      await fetchConnections()
    }
    return res
  }

  const testConnection = async (id: number) => {
    return await testConnectionApi(id)
  }

  const testConnectionDto = async (data: Partial<Connection>) => {
    return await testConnectionDtoApi(data)
  }

  return {
    connections,
    loading,
    fetchConnections,
    createConnection,
    updateConnection,
    deleteConnection,
    testConnection,
    testConnectionDto
  }
})
