import mysql from 'mysql2/promise';
import { IDbProvider, BackupCommand, TableInfo, TableSchema, PagedResult } from './IDbProvider';
import { CryptoService } from '../services/cryptoService';

export class MySqlProvider implements IDbProvider {
  public dbType = 'MySQL';
  private cryptoService: CryptoService;

  constructor(cryptoService: CryptoService) {
    this.cryptoService = cryptoService;
  }

  private getConnConfig(connection: any, databaseOverride?: string) {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    return {
      host: connection.Host || connection.host,
      port: (connection.Port || connection.port) || 3306,
      user: connection.Username || connection.username,
      password: rawPassword,
      database: databaseOverride || (connection.DatabaseName || connection.databaseName) || undefined,
      connectTimeout: 10000
    };
  }

  public async testConnection(connection: any): Promise<boolean> {
    try {
      const conn = await mysql.createConnection(this.getConnConfig(connection));
      await conn.ping();
      await conn.end();
      return true;
    } catch {
      return false;
    }
  }

  public async getDatabases(connection: any): Promise<string[]> {
    const conn = await mysql.createConnection(this.getConnConfig(connection));
    const [rows]: any = await conn.query("SHOW DATABASES WHERE `Database` NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys');");
    await conn.end();
    return rows.map((r: any) => r.Database);
  }

  public async getTables(connection: any, database: string): Promise<TableInfo[]> {
    const conn = await mysql.createConnection(this.getConnConfig(connection, database));
    const [rows]: any = await conn.query('SHOW TABLES;');
    await conn.end();

    const key = Object.keys(rows[0] || {})[0];
    return rows.map((r: any) => ({ name: r[key] }));
  }

  public async getTableSchema(connection: any, database: string, table: string): Promise<TableSchema> {
    const conn = await mysql.createConnection(this.getConnConfig(connection, database));
    const [rows]: any = await conn.query(`DESCRIBE \`${table}\`;`);
    await conn.end();

    return {
      tableName: table,
      columns: rows.map((r: any) => ({
        name: r.Field,
        dataType: r.Type,
        isNullable: r.Null === 'YES',
        isPrimaryKey: r.Key === 'PRI'
      }))
    };
  }

  public async getTableData(connection: any, database: string, table: string, page: number, pageSize: number): Promise<PagedResult> {
    const conn = await mysql.createConnection(this.getConnConfig(connection, database));
    const offset = (page - 1) * pageSize;

    const [countRows]: any = await conn.query(`SELECT COUNT(*) as total FROM \`${table}\`;`);
    const totalCount = countRows[0].total;

    const [rows]: any = await conn.query(`SELECT * FROM \`${table}\` LIMIT ? OFFSET ?;`, [pageSize, offset]);
    await conn.end();

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
    const port = (connection.Port || connection.port) || 3306;
    const user = connection.Username || connection.username;

    let args = `-h "${host}" -P ${port} -u "${user}" --default-character-set=utf8mb4 --result-file="${outputPath}"`;
    if (rawPassword) {
      args += ` -p"${rawPassword}"`;
    }

    args += ` "${database}"`;
    if (tables && tables.length > 0) {
      args += ' ' + tables.map(t => `"${t}"`).join(' ');
    }

    return {
      executable: 'mysqldump',
      arguments: args,
      outputFilePath: outputPath
    };
  }

  public getRestoreCommand(connection: any, database: string, inputPath: string): BackupCommand {
    const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
    const host = connection.Host || connection.host;
    const port = (connection.Port || connection.port) || 3306;
    const user = connection.Username || connection.username;

    let args = `-h "${host}" -P ${port} -u "${user}"`;
    if (rawPassword) {
      args += ` -p"${rawPassword}"`;
    }

    args += ` "${database}" < "${inputPath}"`;

    return {
      executable: 'mysql',
      arguments: args,
      outputFilePath: inputPath
    };
  }
}
