export interface BackupCommand {
  executable: string;
  arguments: string;
  environmentVariables?: Record<string, string>;
  outputFilePath: string;
}

export interface TableInfo {
  name: string;
  rowCount?: number;
}

export interface ColumnSchema {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey?: boolean;
}

export interface TableSchema {
  tableName: string;
  columns: ColumnSchema[];
}

export interface PagedResult<T = any> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface IDbProvider {
  dbType: string;
  testConnection(connection: any): Promise<boolean>;
  getDatabases(connection: any): Promise<string[]>;
  getTables(connection: any, database: string): Promise<TableInfo[]>;
  getTableSchema(connection: any, database: string, table: string): Promise<TableSchema>;
  getTableData(connection: any, database: string, table: string, page: number, pageSize: number): Promise<PagedResult>;
  getBackupCommand(connection: any, database: string, tables: string[] | undefined, outputPath: string): BackupCommand;
  getRestoreCommand(connection: any, database: string, inputPath: string): BackupCommand;
}
