import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Schedule } from '../types/schedule'
import {
  getSchedulesApi,
  createScheduleApi,
  updateScheduleApi,
  deleteScheduleApi,
  toggleScheduleApi,
  runNowScheduleApi
} from '../api/schedule'

export const useScheduleStore = defineStore('schedule', () => {
  const schedules = ref<Schedule[]>([])
  const loading = ref(false)

  const fetchSchedules = async () => {
    loading.value = true
    try {
      const res = await getSchedulesApi()
      if (res && res.success) {
        schedules.value = res.data
      }
    } finally {
      loading.value = false
    }
  }

  const createSchedule = async (data: Partial<Schedule>) => {
    const res = await createScheduleApi(data)
    if (res && res.success) {
      await fetchSchedules()
    }
    return res
  }

  const updateSchedule = async (id: number, data: Partial<Schedule>) => {
    const res = await updateScheduleApi(id, data)
    if (res && res.success) {
      await fetchSchedules()
    }
    return res
  }

  const deleteSchedule = async (id: number) => {
    const res = await deleteScheduleApi(id)
    if (res && res.success) {
      await fetchSchedules()
    }
    return res
  }

  const toggleSchedule = async (id: number) => {
    const res = await toggleScheduleApi(id)
    if (res && res.success) {
      await fetchSchedules()
    }
    return res
  }

  const runNowSchedule = async (id: number) => {
    return await runNowScheduleApi(id)
  }

  return {
    schedules,
    loading,
    fetchSchedules,
    createSchedule,
    updateSchedule,
    deleteSchedule,
    toggleSchedule,
    runNowSchedule
  }
})
