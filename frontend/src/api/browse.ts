import axios from 'axios'

export const getDatabasesApi = async (connectionId: number) => {
  const res = await axios.get(`/api/browse/${connectionId}/databases`)
  return res.data
}

export const getTablesApi = async (connectionId: number, database: string) => {
  const res = await axios.get(`/api/browse/${connectionId}/${database}/tables`)
  return res.data
}

export const getTableSchemaApi = async (connectionId: number, database: string, table: string) => {
  const res = await axios.get(`/api/browse/${connectionId}/${database}/${table}/schema`)
  return res.data
}

export const getTableDataApi = async (
  connectionId: number,
  database: string,
  table: string,
  page = 1,
  pageSize = 50
) => {
  const res = await axios.get(`/api/browse/${connectionId}/${database}/${table}/data`, {
    params: { page, pageSize }
  })
  return res.data
}
