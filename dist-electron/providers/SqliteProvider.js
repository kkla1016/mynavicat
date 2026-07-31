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
exports.SqliteProvider = void 0;
const path = __importStar(require("path"));
const sqlite3_1 = __importDefault(require("sqlite3"));
class SqliteProvider {
    dbType = 'SQLite';
    executeQuery(dbPath, query, params = []) {
        return new Promise((resolve, reject) => {
            const db = new sqlite3_1.default.Database(dbPath, (err) => {
                if (err)
                    return reject(err);
                db.all(query, params, (queryErr, rows) => {
                    db.close();
                    if (queryErr)
                        return reject(queryErr);
                    resolve(rows);
                });
            });
        });
    }
    async testConnection(connection) {
        try {
            const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
            await this.executeQuery(dbPath, 'SELECT 1;');
            return true;
        }
        catch {
            return false;
        }
    }
    getDatabases(connection) {
        const dbPath = connection.DatabaseName || connection.databaseName || 'main';
        return Promise.resolve([path.basename(dbPath)]);
    }
    async getTables(connection, database) {
        const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
        const rows = await this.executeQuery(dbPath, "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");
        return rows.map(r => ({ name: r.name }));
    }
    async getTableSchema(connection, database, table) {
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
    async getTableData(connection, database, table, page, pageSize) {
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
    getBackupCommand(connection, database, tables, outputPath) {
        const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
        const args = `"${dbPath}" ".dump" > "${outputPath}"`;
        return {
            executable: 'sqlite3',
            arguments: args,
            outputFilePath: outputPath
        };
    }
    getRestoreCommand(connection, database, inputPath) {
        const dbPath = connection.DatabaseName || connection.databaseName || 'app.db';
        const args = `"${dbPath}" ".read '${inputPath}'"`;
        return {
            executable: 'sqlite3',
            arguments: args,
            outputFilePath: inputPath
        };
    }
}
exports.SqliteProvider = SqliteProvider;
