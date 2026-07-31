import { ref, onMounted, onUnmounted } from 'vue'
import * as signalR from '@microsoft/signalr'
import { useDesktopNotification } from './useDesktopNotification'
import { ElNotification } from 'element-plus'

export function useSignalR() {
  const isConnected = ref(false)
  let connection: signalR.HubConnection | null = null
  const { sendNotification, requestPermission } = useDesktopNotification()

  const initSignalR = async () => {
    await requestPermission()

    connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notification')
      .withAutomaticReconnect()
      .build()

    connection.on('BackupCompleted', (data: any) => {
      ElNotification({
        title: '🔔 排程備份成功',
        message: data.message || `資料庫 ${data.databaseName} 備份完成`,
        type: 'success',
        duration: 5000
      })

      sendNotification('🔔 MyNavicat 排程備份成功', {
        body: data.message || `資料庫 ${data.databaseName} 備份完成`
      })
    })

    connection.on('BackupFailed', (data: any) => {
      ElNotification({
        title: '🚨 排程備份失敗',
        message: data.errorMessage || data.message || '備份過程發生錯誤',
        type: 'error',
        duration: 0
      })

      sendNotification('🚨 MyNavicat 排程備份失敗', {
        body: data.errorMessage || data.message || '備份過程發生錯誤'
      })
    })

    try {
      await connection.start()
      isConnected.value = true
    } catch (err) {
      console.error('SignalR Connection Error: ', err)
      isConnected.value = false
    }
  }

  onMounted(() => {
    initSignalR()
  })

  onUnmounted(() => {
    if (connection) {
      connection.stop()
    }
  })

  return {
    isConnected
  }
}
