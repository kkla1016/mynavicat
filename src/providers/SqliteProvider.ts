import * as path from 'path';
import sqlite3 from 'sqlite3';
import { IDbProvider, BackupCommand, TableInfo, TableSchema, PagedResult } from './IDbProvider';

export class SqliteProvider implements IDbProvider {
  public dbType = 'SQLite';

  private executeQuery(dbPath: string, query: string, params: any[] = []): Promise<any[]> {
    return new Promise((resolve, reject) => {
      const db = new sqlite3.Database(dbPath, (err) => {
        if (err) return reject(err);
        db.all(query, params, (queryErr, rows) => {
          db.close();
          if (queryErr) return reject(queryErr);
          resolve(rows);
        });
      });
    });
  }

  public async testConnection(connection: any): Promise<boolean> {
    try {
      const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
      await this.executeQuery(dbPath, 'SELECT 1;');
      return true;
    } catch {
      return false;
    }
  }

  public getDatabases(connection: any): Promise<string[]> {
    const dbPath = connection.DatabaseName || connection.databaseName || 'main';
    return Promise.resolve([path.basename(dbPath)]);
  }

  public async getTables(connection: any, database: string): Promise<TableInfo[]> {
    const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
    const rows = await this.executeQuery(dbPath, "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
    return rows.map(r => ({ name: r.name }));
  }

  public async getTableSchema(connection: any, database: string, table: string): Promise<TableSchema> {
    const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
    const rows = await this.executeQuery(dbPath, `PRAGMA table_info('${table}');`);

    return {
      tableName: table,
      columns: rows.map(r => ({
        name: r.name,
        dataType: r.type,
        isNullable: r.notnull === 0,
        isPrimaryKey: r.pk === 1
      }))
    };
  }

  public async getTableData(connection: any, database: string, table: string, page: number, pageSize: number): Promise<PagedResult> {
    const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
    const offset = (page - 1) * pageSize;

    const countRows = await this.executeQuery(dbPath, `SELECT COUNT(*) as total FROM "${table}";`);
    const totalCount = countRows[0].total;

    const rows = await this.executeQuery(dbPath, `SELECT * FROM "${table}" LIMIT ? OFFSET ?;`, [pageSize, offset]);

    return {
      items: rows,
      page,
      pageSize,
      totalCount
    };
  }

  public getBackupCommand(connection: any, database: string, tables: string[] | undefined, outputPath: string): BackupCommand {
    const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
    const args = `"${dbPath}" ".dump" > "${outputPath}"`;

    return {
      executable: 'sqlite3',
      arguments: args,
      outputFilePath: outputPath
    };
  }

  public getRestoreCommand(connection: any, database: string, inputPath: string): BackupCommand {
    const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
    const args = `"${dbPath}" ".read '${inputPath}'"`;

    return {
      executable: 'sqlite3',
      arguments: args,
      outputFilePath: inputPath
    };
  }
}
