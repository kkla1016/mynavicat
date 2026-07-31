export interface TableInfo {
  name: string
  comment?: string | null
  rowCount: number
  dataLengthBytes: number
}

export interface ColumnSchema {
  name: string
  dataType: string
  isNullable: boolean
  isPrimaryKey: boolean
  defaultValue?: string | null
  comment?: string | null
}

export interface IndexSchema {
  name: string
  isUnique: boolean
  columns: string[]
}

export interface TableSchema {
  tableName: string
  columns: ColumnSchema[]
  indexes: IndexSchema[]
}

export interface PagedData<T> {
  items: T[]
  pageIndex: number
  pageSize: number
  totalCount: number
  totalPages: number
}
