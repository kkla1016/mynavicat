import { ref } from 'vue'

export function useDesktopNotification() {
  const permission = ref<NotificationPermission>('default')

  const requestPermission = async () => {
    if (!('Notification' in window)) {
      console.warn('Browser does not support desktop notifications.')
      return false
    }

    try {
      const res = await Notification.requestPermission()
      permission.value = res
      return res === 'granted'
    } catch {
      return false
    }
  }

  const sendNotification = (title: string, options?: NotificationOptions) => {
    if (!('Notification' in window) || Notification.permission !== 'granted') {
      return
    }

    try {
      new Notification(title, {
        icon: '/favicon.ico',
        ...options
      })
    } catch (err) {
      console.error('Failed to send notification', err)
    }
  }

  return {
    permission,
    requestPermission,
    sendNotification
  }
}
