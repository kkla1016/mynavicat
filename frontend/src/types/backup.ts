export interface BackupRequest {
  connectionId: number
  databaseName: string
  tables?: string[]
  compress: boolean
  compressionType: 'gz' | 'zip'
}

export interface RestoreRequest {
  backupHistoryId?: number
  targetConnectionId: number
  targetDatabaseName: string
}

export interface BackupHistory {
  id: number
  connectionId: number
  parentBackupId?: number | null
  databaseName: string
  tables?: string | null
  backupType: 'Full' | 'Partial' | 'Chunked'
  filePath: string
  fileSize: number
  isCompressed: boolean
  compressionType?: string | null
  chunkIndex?: number | null
  totalChunks?: number | null
  status: 'Success' | 'Failed' | 'InProgress'
  errorMessage?: string | null
  scheduleId?: number | null
  startedAt: string
  completedAt?: string | null
  durationSeconds: number
  connection?: {
    name: string
    dbType: string
  }
}
