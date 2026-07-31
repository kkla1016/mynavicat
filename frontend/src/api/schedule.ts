import axios from 'axios'
import type { Schedule } from '../types/schedule'

export const getSchedulesApi = async () => {
  const res = await axios.get('/api/schedules')
  return res.data
}

export const createScheduleApi = async (data: Partial<Schedule>) => {
  const res = await axios.post('/api/schedules', data)
  return res.data
}

export const updateScheduleApi = async (id: number, data: Partial<Schedule>) => {
  const res = await axios.put(`/api/schedules/${id}`, data)
  return res.data
}

export const deleteScheduleApi = async (id: number) => {
  const res = await axios.delete(`/api/schedules/${id}`)
  return res.data
}

export const toggleScheduleApi = async (id: number) => {
  const res = await axios.post(`/api/schedules/${id}/toggle`)
  return res.data
}

export const runNowScheduleApi = async (id: number) => {
  const res = await axios.post(`/api/schedules/${id}/run-now`)
  return res.data
}
