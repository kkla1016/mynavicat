"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.StorageService = void 0;
const path = __importStar(require("path"));
const fs = __importStar(require("fs"));
const better_sqlite3_1 = __importDefault(require("better-sqlite3"));
class StorageService {
    db;
    constructor(customPath) {
        const userDataPath = customPath || path.join(process.env.APPDATA || process.env.HOME || '.', 'MyNavicat');
        if (!fs.existsSync(userDataPath)) {
            fs.mkdirSync(userDataPath, { recursive: true });
        }
        const dbPath = path.join(userDataPath, 'app.db');
        this.db = new better_sqlite3_1.default(dbPath);
        this.initDatabase();
    }
    initDatabase() {
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
    getConnections() {
        const stmt = this.db.prepare('SELECT * FROM Connections ORDER BY Id DESC');
        return stmt.all();
    }
    getConnectionById(id) {
        const stmt = this.db.prepare('SELECT * FROM Connections WHERE Id = ?');
        return stmt.get(id);
    }
    saveConnection(conn) {
        if (conn.Id || conn.id) {
            const id = conn.Id || conn.id;
            const stmt = this.db.prepare(`
        UPDATE Connections SET
          Name = ?, DbType = ?, Host = ?, Port = ?, Username = ?,
          EncryptedPassword = ?, DatabaseName = ?, ExtraParams = ?
        WHERE Id = ?
      `);
            stmt.run(conn.Name || conn.name, conn.DbType || conn.dbType, conn.Host || conn.host, conn.Port || conn.port, conn.Username || conn.username, conn.EncryptedPassword || conn.encryptedPassword, conn.DatabaseName || conn.databaseName || '', conn.ExtraParams || conn.extraParams || '', id);
            return this.getConnectionById(id);
        }
        else {
            const stmt = this.db.prepare(`
        INSERT INTO Connections (Name, DbType, Host, Port, Username, EncryptedPassword, DatabaseName, ExtraParams)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
      `);
            const info = stmt.run(conn.Name || conn.name, conn.DbType || conn.dbType, conn.Host || conn.host, conn.Port || conn.port, conn.Username || conn.username, conn.EncryptedPassword || conn.encryptedPassword, conn.DatabaseName || conn.databaseName || '', conn.ExtraParams || conn.extraParams || '');
            return this.getConnectionById(info.lastInsertRowid);
        }
    }
    deleteConnection(id) {
        const stmt = this.db.prepare('DELETE FROM Connections WHERE Id = ?');
        const result = stmt.run(id);
        return result.changes > 0;
    }
    // BackupHistory CRUD
    getBackupHistories(connectionId, status) {
        let sql = 'SELECT h.*, c.Name as ConnectionName FROM BackupHistories h LEFT JOIN Connections c ON h.ConnectionId = c.Id WHERE h.ParentBackupId IS NULL';
        const params = [];
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
    saveBackupHistory(history) {
        if (history.Id || history.id) {
            const id = history.Id || history.id;
            const stmt = this.db.prepare(`
        UPDATE BackupHistories SET
          Status = ?, FilePath = ?, FileSize = ?, IsCompressed = ?, CompressionType = ?,
          ErrorMessage = ?, CompletedAt = ?, DurationSeconds = ?, TotalChunks = ?
        WHERE Id = ?
      `);
            stmt.run(history.Status || history.status, history.FilePath || history.filePath, history.FileSize || history.fileSize || 0, history.IsCompressed ? 1 : 0, history.CompressionType || history.compressionType || '', history.ErrorMessage || history.errorMessage || '', history.CompletedAt || history.completedAt, history.DurationSeconds || history.durationSeconds || 0, history.TotalChunks || history.totalChunks || null, id);
            return history;
        }
        else {
            const stmt = this.db.prepare(`
        INSERT INTO BackupHistories (
          ConnectionId, ParentBackupId, DatabaseName, Tables, BackupType, FilePath,
          FileSize, IsCompressed, CompressionType, ChunkIndex, TotalChunks, Status,
          ErrorMessage, StartedAt, CompletedAt, DurationSeconds, ScheduleId
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
      `);
            const info = stmt.run(history.ConnectionId || history.connectionId, history.ParentBackupId || history.parentBackupId || null, history.DatabaseName || history.databaseName, history.Tables || history.tables || null, history.BackupType || history.backupType || 'Full', history.FilePath || history.filePath, history.FileSize || history.fileSize || 0, history.IsCompressed ? 1 : 0, history.CompressionType || history.compressionType || '', history.ChunkIndex !== undefined ? history.ChunkIndex : null, history.TotalChunks || history.totalChunks || null, history.Status || history.status || 'InProgress', history.ErrorMessage || history.errorMessage || '', history.StartedAt || history.startedAt || new Date().toISOString(), history.CompletedAt || history.completedAt || null, history.DurationSeconds || history.durationSeconds || 0, history.ScheduleId || history.scheduleId || null);
            return { ...history, Id: info.lastInsertRowid };
        }
    }
    deleteBackupHistory(id) {
        const stmt = this.db.prepare('DELETE FROM BackupHistories WHERE Id = ? OR ParentBackupId = ?');
        const result = stmt.run(id, id);
        return result.changes > 0;
    }
    // BackupSchedules CRUD
    getSchedules() {
        const stmt = this.db.prepare('SELECT s.*, c.Name as ConnectionName FROM BackupSchedules s LEFT JOIN Connections c ON s.ConnectionId = c.Id ORDER BY s.Id DESC');
        return stmt.all();
    }
    saveSchedule(schedule) {
        if (schedule.Id || schedule.id) {
            const id = schedule.Id || schedule.id;
            const stmt = this.db.prepare(`
        UPDATE BackupSchedules SET
          ConnectionId = ?, DatabaseName = ?, CronExpression = ?, Compress = ?,
          CompressionType = ?, RetainCount = ?, IsEnabled = ?
        WHERE Id = ?
      `);
            stmt.run(schedule.ConnectionId || schedule.connectionId, schedule.DatabaseName || schedule.databaseName, schedule.CronExpression || schedule.cronExpression, schedule.Compress ? 1 : 0, schedule.CompressionType || schedule.compressionType || 'gz', schedule.RetainCount || schedule.retainCount || 7, schedule.IsEnabled ? 1 : 0, id);
            return schedule;
        }
        else {
            const stmt = this.db.prepare(`
        INSERT INTO BackupSchedules (ConnectionId, DatabaseName, CronExpression, Compress, CompressionType, RetainCount, IsEnabled)
        VALUES (?, ?, ?, ?, ?, ?, ?)
      `);
            const info = stmt.run(schedule.ConnectionId || schedule.connectionId, schedule.DatabaseName || schedule.databaseName, schedule.CronExpression || schedule.cronExpression, schedule.Compress ? 1 : 0, schedule.CompressionType || schedule.compressionType || 'gz', schedule.RetainCount || schedule.retainCount || 7, schedule.IsEnabled ? 1 : 0);
            return { ...schedule, Id: info.lastInsertRowid };
        }
    }
    deleteSchedule(id) {
        const stmt = this.db.prepare('DELETE FROM BackupSchedules WHERE Id = ?');
        const result = stmt.run(id);
        return result.changes > 0;
    }
}
exports.StorageService = StorageService;
