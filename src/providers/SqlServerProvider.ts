import { Connection, Request } from 'tedious';
import { IDbProvider, BackupCommand, TableInfo, TableSchema, PagedResult } from './IDbProvider';
import { CryptoService } from '../services/cryptoService';

export class SqlServerProvider implements IDbProvider {
  public dbType = 'SqlServer';
  private cryptoService: CryptoService;

  constructor(cryptoService: CryptoService) {
    this.cryptoService = cryptoService;
  }

  private getConfig(connection: any, databaseOverride?: string): any {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    return {
      server: connection.Host || connection.host,
      options: {
        port: (connection.Port || connection.port) || 1433,
        database: databaseOverride || (connection.DatabaseName || connection.databaseName) || 'master',
        encrypt: false,
        trustServerCertificate: true,
        connectTimeout: 10000
      },
      authentication: {
        type: 'default' as const,
        options: {
          userName: connection.Username || connection.username,
          password: rawPassword
        }
      }
    };
  }

  private executeQuery(connection: any, query: string, databaseOverride?: string): Promise<any[]> {
    return new Promise((resolve, reject) => {
      const config = this.getConfig(connection, databaseOverride);
      const conn = new Connection(config);

      conn.on('connect', (err) => {
        if (err) return reject(err);

        const rows: any[] = [];
        const request = new Request(query, (reqErr) => {
          conn.close();
          if (reqErr) return reject(reqErr);
          resolve(rows);
        });

        request.on('row', (columns: any[]) => {
          const rowObj: any = {};
          columns.forEach((col: any) => {
            rowObj[col.metadata.colName] = col.value;
          });
          rows.push(rowObj);
        });

        conn.execSql(request);
      });

      conn.connect();
    });
  }

  public async testConnection(connection: any): Promise<boolean> {
    try {
      await this.executeQuery(connection, 'SELECT 1 as ok');
      return true;
    } catch {
      return false;
    }
  }

  public async getDatabases(connection: any): Promise<string[]> {
    const rows = await this.executeQuery(connection, "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');", 'master');
    return rows.map(r => r.name);
  }

  public async getTables(connection: any, database: string): Promise<TableInfo[]> {
    const rows = await this.executeQuery(connection, "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';", database);
    return rows.map(r => ({ name: r.TABLE_NAME }));
  }

  public async getTableSchema(connection: any, database: string, table: string): Promise<TableSchema> {
    const rows = await this.executeQuery(connection, `
      SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
      FROM INFORMATION_SCHEMA.COLUMNS
      WHERE TABLE_NAME = '${table}';
    `, database);

    return {
      tableName: table,
      columns: rows.map(r => ({
        name: r.COLUMN_NAME,
        dataType: r.DATA_TYPE,
        isNullable: r.IS_NULLABLE === 'YES'
      }))
    };
  }

  public async getTableData(connection: any, database: string, table: string, page: number, pageSize: number): Promise<PagedResult> {
    const offset = (page - 1) * pageSize;
    const countRows = await this.executeQuery(connection, `SELECT COUNT(*) as total FROM [${table}];`, database);
    const totalCount = countRows[0].total;

    const rows = await this.executeQuery(connection, `
      SELECT * FROM [${table}] ORDER BY (SELECT NULL) OFFSET ${offset} ROWS FETCH NEXT ${pageSize} ROWS ONLY;
    `, database);

    return {
      items: rows,
      page,
      pageSize,
      totalCount
    };
  }

  public getBackupCommand(connection: any, database: string, tables: string[] | undefined, outputPath: string): BackupCommand {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    const host = connection.Host || connection.host;
    const port = (connection.Port || connection.port) || 1433;
    const user = connection.Username || connection.username;

    const args = `-S "${host},${port}" -U "${user}" -P "${rawPassword}" -Q "BACKUP DATABASE [${database}] TO DISK = N'${outputPath}' WITH FORMAT, INIT;"`;

    return {
      executable: 'sqlcmd',
      arguments: args,
      outputFilePath: outputPath
    };
  }

  public getRestoreCommand(connection: any, database: string, inputPath: string): BackupCommand {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    const host = connection.Host || connection.host;
    const port = (connection.Port || connection.port) || 1433;
    const user = connection.Username || connection.username;

    const args = `-S "${host},${port}" -U "${user}" -P "${rawPassword}" -Q "RESTORE DATABASE [${database}] FROM DISK = N'${inputPath}' WITH REPLACE;"`;

    return {
      executable: 'sqlcmd',
      arguments: args,
      outputFilePath: inputPath
    };
  }
}
