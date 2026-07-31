import { IDbProvider, BackupCommand, TableInfo, TableSchema, PagedResult } from './IDbProvider';
import { MySqlProvider } from './MySqlProvider';
import { CryptoService } from '../services/cryptoService';

export class MariaDbProvider implements IDbProvider {
  public dbType = 'MariaDB';
  private innerProvider: MySqlProvider;

  constructor(cryptoService: CryptoService) {
    this.innerProvider = new MySqlProvider(cryptoService);
  }

  public testConnection(connection: any): Promise<boolean> {
    return this.innerProvider.testConnection(connection);
  }

  public getDatabases(connection: any): Promise<string[]> {
    return this.innerProvider.getDatabases(connection);
  }

  public getTables(connection: any, database: string): Promise<TableInfo[]> {
    return this.innerProvider.getTables(connection, database);
  }

  public getTableSchema(connection: any, database: string, table: string): Promise<TableSchema> {
    return this.innerProvider.getTableSchema(connection, database, table);
  }

  public getTableData(connection: any, database: string, table: string, page: number, pageSize: number): Promise<PagedResult> {
    return this.innerProvider.getTableData(connection, database, table, page, pageSize);
  }

  public getBackupCommand(connection: any, database: string, tables: string[] | undefined, outputPath: string): BackupCommand {
    const cmd = this.innerProvider.getBackupCommand(connection, database, tables, outputPath);
    cmd.executable = 'mariadb-dump';
    return cmd;
  }

  public getRestoreCommand(connection: any, database: string, inputPath: string): BackupCommand {
    const cmd = this.innerProvider.getRestoreCommand(connection, database, inputPath);
    cmd.executable = 'mariadb';
    return cmd;
  }
}
