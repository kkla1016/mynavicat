import axios from 'axios'
import type { BackupRequest, RestoreRequest } from '../types/backup'

export const executeBackupApi = async (data: BackupRequest) => {
  const res = await axios.post('/api/backup', data)
  return res.data
}

export const executeRestoreApi = async (data: RestoreRequest) => {
  const res = await axios.post('/api/backup/restore', data)
  return res.data
}

export const executeRestoreFromFileApi = async (formData: FormData) => {
  const res = await axios.post('/api/backup/restore-file', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return res.data
}

export const getBackupHistoryApi = async (connectionId?: number, status?: string, page = 1, pageSize = 20) => {
  const res = await axios.get('/api/backup/history', {
    params: { connectionId, status, page, pageSize }
  })
  return res.data
}

export const deleteBackupHistoryApi = async (id: number) => {
  const res = await axios.delete(`/api/backup/history/${id}`)
  return res.data
}

export const getBackupDownloadUrl = (id: number) => {
  return `/api/backup/history/${id}/download`
}
