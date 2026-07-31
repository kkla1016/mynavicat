export async function getDatabasesApi(connectionId: number): Promise<any> {
  if (window.electronAPI) {
    const data = await window.electronAPI.getDatabases(connectionId)
    return { success: true, data }
  }
  return { success: false, data: [] }
}

export async function getTablesApi(connectionId: number, database: string): Promise<any> {
  if (window.electronAPI) {
    const data = await window.electronAPI.getTables(connectionId, database)
    return { success: true, data }
  }
  return { success: false, data: [] }
}

export async function getTableSchemaApi(connectionId: number, database: string, table: string): Promise<any> {
  if (window.electronAPI) {
    const data = await window.electronAPI.getTableSchema(connectionId, database, table)
    return { success: true, data }
  }
  return { success: false, data: null }
}

export async function getTableDataApi(connectionId: number, database: string, table: string, page: number = 1, pageSize: number = 20): Promise<any> {
  if (window.electronAPI) {
    const data = await window.electronAPI.getTableData(connectionId, database, table, page, pageSize)
    return { success: true, data }
  }
  return { success: false, data: { items: [], page: 1, pageSize: 20, totalCount: 0 } }
}
