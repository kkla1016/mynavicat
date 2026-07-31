import axios from 'axios'
import type { Connection } from '../types/connection'

export const getConnectionsApi = async () => {
  const res = await axios.get('/api/connections')
  return res.data
}

export const getConnectionByIdApi = async (id: number) => {
  const res = await axios.get(`/api/connections/${id}`)
  return res.data
}

export const createConnectionApi = async (data: Partial<Connection>) => {
  const res = await axios.post('/api/connections', data)
  return res.data
}

export const updateConnectionApi = async (id: number, data: Partial<Connection>) => {
  const res = await axios.put(`/api/connections/${id}`, data)
  return res.data
}

export const deleteConnectionApi = async (id: number) => {
  const res = await axios.delete(`/api/connections/${id}`)
  return res.data
}

export const testConnectionApi = async (id: number) => {
  const res = await axios.post(`/api/connections/${id}/test`)
  return res.data
}

export const testConnectionDtoApi = async (data: Partial<Connection>) => {
  const res = await axios.post('/api/connections/test-dto', data)
  return res.data
}
