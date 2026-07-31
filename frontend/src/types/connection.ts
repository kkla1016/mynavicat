export interface Connection {
  id: number
  name: string
  dbType: string
  host: string
  port: number
  databaseName: string
  username: string
  password?: string
  sshHost?: string
  sshPort?: number
  sshUsername?: string
  sshKeyPath?: string
  extraParams?: string
  isActive: boolean
  createdAt?: string
  updatedAt?: string
}

export type DbTypeOption = 'MySQL' | 'PostgreSQL' | 'SqlServer' | 'MariaDB' | 'SQLite' | 'Oracle'
