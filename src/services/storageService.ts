import * as path from 'path';
import * as fs from 'fs';
import Database from 'better-sqlite3';

export class StorageService {
  private db: Database.Database;

  constructor(customPath?: string) {
    const userDataPath = customPath || path.join(process.env.APPDATA || process.env.HOME || '.', 'MyNavicat');
    if (!fs.existsSync(userDataPath)) {
      fs.mkdirSync(userDataPath, { recursive: true });
    }

    const dbPath = path.join(userDataPath, 'app.db');
    this.db = new Database(dbPath);
    this.initDatabase();
  }

  private initDatabase() {
    this.db.exec(`
      CREATE TABLE IF NOT EXISTS Connections (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        DbType TEXT NOT NULL,
        Host TEXT NOT NULL,
        Port INTEGER NOT NULL,
        Username TEXT NOT NULL,
        EncryptedPassword TEXT NOT NULL,
        DatabaseName TEXT,
        ExtraParams TEXT,
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
      );

      CREATE TABLE IF NOT EXISTS BackupSchedules (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        ConnectionId INTEGER NOT NULL,
        DatabaseName TEXT NOT NULL,
        CronExpression TEXT NOT NULL,
        Compress INTEGER DEFAULT 1,
        CompressionType TEXT DEFAULT 'gz',
        RetainCount INTEGER DEFAULT 7,
        IsEnabled INTEGER DEFAULT 1,
        LastRunAt DATETIME,
        NextRunAt DATETIME,
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (ConnectionId) REFERENCES Connections(Id) ON DELETE CASCADE
      );

      CREATE TABLE IF NOT EXISTS BackupHistories (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        ConnectionId INTEGER NOT NULL,
        ParentBackupId INTEGER,
        DatabaseName TEXT NOT NULL,
        Tables TEXT,
        BackupType TEXT DEFAULT 'Full',
        FilePath TEXT NOT NULL,
        FileSize INTEGER DEFAULT 0,
        IsCompressed INTEGER DEFAULT 0,
        CompressionType TEXT,
        ChunkIndex INTEGER,
        TotalChunks INTEGER,
        Status TEXT NOT NULL,
        ErrorMessage TEXT,
        StartedAt DATETIME,
        CompletedAt DATETIME,
        DurationSeconds REAL,
        ScheduleId INTEGER,
        FOREIGN KEY (ConnectionId) REFERENCES Connections(Id) ON DELETE CASCADE
      );
    `);
  }

  // Connection CRUD
  public getConnections(): any[] {
    const stmt = this.db.prepare('SELECT * FROM Connections ORDER BY Id DESC');
    return stmt.all();
  }

  public getConnectionById(id: number): any {
    const stmt = this.db.prepare('SELECT * FROM Connections WHERE Id = ?');
    return stmt.get(id);
  }

  public saveConnection(conn: any): any {
    if (conn.Id || conn.id) {
      const id = conn.Id || conn.id;
      const stmt = this.db.prepare(`
        UPDATE Connections SET
          Name = ?, DbType = ?, Host = ?, Port = ?, Username = ?,
          EncryptedPassword = ?, DatabaseName = ?, ExtraParams = ?
        WHERE Id = ?
      `);
      stmt.run(
        conn.Name || conn.name,
        conn.DbType || conn.dbType,
        conn.Host || conn.host,
        conn.Port || conn.port,
        conn.Username || conn.username,
        conn.EncryptedPassword || conn.encryptedPassword,
        conn.DatabaseName || conn.databaseName || '',
        conn.ExtraParams || conn.extraParams || '',
        id
      );
      return this.getConnectionById(id);
    } else {
      const stmt = this.db.prepare(`
        INSERT INTO Connections (Name, DbType, Host, Port, Username, EncryptedPassword, DatabaseName, ExtraParams)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
      `);
      const info = stmt.run(
        conn.Name || conn.name,
        conn.DbType || conn.dbType,
        conn.Host || conn.host,
        conn.Port || conn.port,
        conn.Username || conn.username,
        conn.EncryptedPassword || conn.encryptedPassword,
        conn.DatabaseName || conn.databaseName || '',
        conn.ExtraParams || conn.extraParams || ''
      );
      return this.getConnectionById(info.lastInsertRowid as number);
    }
  }

  public deleteConnection(id: number): boolean {
    const stmt = this.db.prepare('DELETE FROM Connections WHERE Id = ?');
    const result = stmt.run(id);
    return result.changes > 0;
  }

  // BackupHistory CRUD
  public getBackupHistories(connectionId?: number, status?: string): any[] {
    let sql = 'SELECT h.*, c.Name as ConnectionName FROM BackupHistories h LEFT JOIN Connections c ON h.ConnectionId = c.Id WHERE h.ParentBackupId IS NULL';
    const params: any[] = [];

    if (connectionId) {
      sql += ' AND h.ConnectionId = ?';
      params.push(connectionId);
    }
    if (status) {
      sql += ' AND h.Status = ?';
      params.push(status);
    }

    sql += ' ORDER BY h.StartedAt DESC';
    const stmt = this.db.prepare(sql);
    return stmt.all(...params);
  }

  public saveBackupHistory(history: any): any {
    if (history.Id || history.id) {
      const id = history.Id || history.id;
      const stmt = this.db.prepare(`
        UPDATE BackupHistories SET
          Status = ?, FilePath = ?, FileSize = ?, IsCompressed = ?, CompressionType = ?,
          ErrorMessage = ?, CompletedAt = ?, DurationSeconds = ?, TotalChunks = ?
        WHERE Id = ?
      `);
      stmt.run(
        history.Status || history.status,
        history.FilePath || history.filePath,
        history.FileSize || history.fileSize || 0,
        history.IsCompressed ? 1 : 0,
        history.CompressionType || history.compressionType || '',
        history.ErrorMessage || history.errorMessage || '',
        history.CompletedAt || history.completedAt,
        history.DurationSeconds || history.durationSeconds || 0,
        history.TotalChunks || history.totalChunks || null,
        id
      );
      return history;
    } else {
      const stmt = this.db.prepare(`
        INSERT INTO BackupHistories (
          ConnectionId, ParentBackupId, DatabaseName, Tables, BackupType, FilePath,
          FileSize, IsCompressed, CompressionType, ChunkIndex, TotalChunks, Status,
          ErrorMessage, StartedAt, CompletedAt, DurationSeconds, ScheduleId
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
      `);
      const info = stmt.run(
        history.ConnectionId || history.connectionId,
        history.ParentBackupId || history.parentBackupId || null,
        history.DatabaseName || history.databaseName,
        history.Tables || history.tables || null,
        history.BackupType || history.backupType || 'Full',
        history.FilePath || history.filePath,
        history.FileSize || history.fileSize || 0,
        history.IsCompressed ? 1 : 0,
        history.CompressionType || history.compressionType || '',
        history.ChunkIndex !== undefined ? history.ChunkIndex : null,
        history.TotalChunks || history.totalChunks || null,
        history.Status || history.status || 'InProgress',
        history.ErrorMessage || history.errorMessage || '',
        history.StartedAt || history.startedAt || new Date().toISOString(),
        history.CompletedAt || history.completedAt || null,
        history.DurationSeconds || history.durationSeconds || 0,
        history.ScheduleId || history.scheduleId || null
      );
      return { ...history, Id: info.lastInsertRowid };
    }
  }

  public deleteBackupHistory(id: number): boolean {
    const stmt = this.db.prepare('DELETE FROM BackupHistories WHERE Id = ? OR ParentBackupId = ?');
    const result = stmt.run(id, id);
    return result.changes > 0;
  }

  // BackupSchedules CRUD
  public getSchedules(): any[] {
    const stmt = this.db.prepare('SELECT s.*, c.Name as ConnectionName FROM BackupSchedules s LEFT JOIN Connections c ON s.ConnectionId = c.Id ORDER BY s.Id DESC');
    return stmt.all();
  }

  public saveSchedule(schedule: any): any {
    if (schedule.Id || schedule.id) {
      const id = schedule.Id || schedule.id;
      const stmt = this.db.prepare(`
        UPDATE BackupSchedules SET
          ConnectionId = ?, DatabaseName = ?, CronExpression = ?, Compress = ?,
          CompressionType = ?, RetainCount = ?, IsEnabled = ?
        WHERE Id = ?
      `);
      stmt.run(
        schedule.ConnectionId || schedule.connectionId,
        schedule.DatabaseName || schedule.databaseName,
        schedule.CronExpression || schedule.cronExpression,
        schedule.Compress ? 1 : 0,
        schedule.CompressionType || schedule.compressionType || 'gz',
        schedule.RetainCount || schedule.retainCount || 7,
        schedule.IsEnabled ? 1 : 0,
        id
      );
      return schedule;
    } else {
      const stmt = this.db.prepare(`
        INSERT INTO BackupSchedules (ConnectionId, DatabaseName, CronExpression, Compress, CompressionType, RetainCount, IsEnabled)
        VALUES (?, ?, ?, ?, ?, ?, ?)
      `);
      const info = stmt.run(
        schedule.ConnectionId || schedule.connectionId,
        schedule.DatabaseName || schedule.databaseName,
        schedule.CronExpression || schedule.cronExpression,
        schedule.Compress ? 1 : 0,
        schedule.CompressionType || schedule.compressionType || 'gz',
        schedule.RetainCount || schedule.retainCount || 7,
        schedule.IsEnabled ? 1 : 0
      );
      return { ...schedule, Id: info.lastInsertRowid };
    }
  }

  public deleteSchedule(id: number): boolean {
    const stmt = this.db.prepare('DELETE FROM BackupSchedules WHERE Id = ?');
    const result = stmt.run(id);
    return result.changes > 0;
  }
}
