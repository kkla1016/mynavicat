import { IDbProvider, BackupCommand, TableInfo, TableSchema, PagedResult } from './IDbProvider';
import { CryptoService } from '../services/cryptoService';

export class OracleProvider implements IDbProvider {
  public dbType = 'Oracle';
  private cryptoService: CryptoService;

  constructor(cryptoService: CryptoService) {
    this.cryptoService = cryptoService;
  }

  public testConnection(connection: any): Promise<boolean> {
    return Promise.resolve(true);
  }

  public getDatabases(connection: any): Promise<string[]> {
    const sid = connection.DatabaseName || connection.databaseName || 'XE';
    return Promise.resolve([sid]);
  }

  public getTables(connection: any, database: string): Promise<TableInfo[]> {
    return Promise.resolve([]);
  }

  public getTableSchema(connection: any, database: string, table: string): Promise<TableSchema> {
    return Promise.resolve({ tableName: table, columns: [] });
  }

  public getTableData(connection: any, database: string, table: string, page: number, pageSize: number): Promise<PagedResult> {
    return Promise.resolve({ items: [], page: 1, pageSize: 20, totalCount: 0 });
  }

  public getBackupCommand(connection: any, database: string, tables: string[] | undefined, outputPath: string): BackupCommand {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    const host = connection.Host || connection.host;
    const port = (connection.Port || connection.port) || 1521;
    const sid = connection.DatabaseName || connection.databaseName || 'XE';
    const user = connection.Username || connection.username;

    const args = `userid=${user}/${rawPassword}@${host}:${port}/${sid} file="${outputPath}" log="${outputPath}.log"`;

    return {
      executable: 'exp',
      arguments: args,
      outputFilePath: outputPath
    };
  }

  public getRestoreCommand(connection: any, database: string, inputPath: string): BackupCommand {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    const host = connection.Host || connection.host;
    const port = (connection.Port || connection.port) || 1521;
    const sid = connection.DatabaseName || connection.databaseName || 'XE';
    const user = connection.Username || connection.username;

    const args = `userid=${user}/${rawPassword}@${host}:${port}/${sid} file="${inputPath}" full=y`;

    return {
      executable: 'imp',
      arguments: args,
      outputFilePath: inputPath
    };
  }
}
