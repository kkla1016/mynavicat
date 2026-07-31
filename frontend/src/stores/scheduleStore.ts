import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useScheduleStore = defineStore('schedule', () => {
  const schedules = ref<any[]>([])
  const loading = ref(false)

  const scheduleList = computed(() => schedules.value)

  const fetchSchedules = async () => {
    loading.value = true
    try {
      if (window.electronAPI) {
        schedules.value = await window.electronAPI.getSchedules()
      }
    } finally {
      loading.value = false
    }
  }

  const saveSchedule = async (schedule: any) => {
    if (window.electronAPI) {
      const result = await window.electronAPI.saveSchedule(schedule)
      await fetchSchedules()
      return { success: true, data: result }
    }
    return { success: false }
  }

  const createSchedule = saveSchedule
  const updateSchedule = async (_id: number, schedule: any) => saveSchedule(schedule)

  const deleteSchedule = async (id: number) => {
    if (window.electronAPI) {
      const ok = await window.electronAPI.deleteSchedule(id)
      if (ok) await fetchSchedules()
      return { success: ok }
    }
    return { success: false }
  }

  const toggleSchedule = async (id: number, isEnabled?: boolean) => {
    if (window.electronAPI) {
      const targetState = isEnabled !== undefined ? isEnabled : true
      const ok = await window.electronAPI.toggleSchedule(id, targetState)
      if (ok) await fetchSchedules()
      return { success: ok }
    }
    return { success: false }
  }

  const runScheduleNow = async (id: number) => {
    if (window.electronAPI) {
      const ok = await window.electronAPI.runScheduleNow(id)
      return { success: ok }
    }
    return { success: false }
  }

  const runNowSchedule = runScheduleNow

  return {
    schedules,
    scheduleList,
    loading,
    fetchSchedules,
    saveSchedule,
    createSchedule,
    updateSchedule,
    deleteSchedule,
    toggleSchedule,
    runScheduleNow,
    runNowSchedule
  }
})
