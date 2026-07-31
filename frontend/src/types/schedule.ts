export interface Schedule {
  id: number
  connectionId: number
  databaseName: string
  tables?: string | null
  cronExpression: string
  description?: string | null
  isEnabled: boolean
  compressBackup: boolean
  compressionType: 'gz' | 'zip'
  retainCount: number
  hangfireJobId?: string | null
  lastRunAt?: string | null
  nextRunAt?: string | null
  createdAt?: string
  updatedAt?: string
  connectionName?: string
}
