"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SqlServerProvider = void 0;
const tedious_1 = require("tedious");
class SqlServerProvider {
    dbType = 'SqlServer';
    cryptoService;
    constructor(cryptoService) {
        this.cryptoService = cryptoService;
    }
    getConfig(connection, databaseOverride) {
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
                type: 'default',
                options: {
                    userName: connection.Username || connection.username,
                    password: rawPassword
                }
            }
        };
    }
    executeQuery(connection, query, databaseOverride) {
        return new Promise((resolve, reject) => {
            const config = this.getConfig(connection, databaseOverride);
            const conn = new tedious_1.Connection(config);
            conn.on('connect', (err) => {
                if (err)
                    return reject(err);
                const rows = [];
                const request = new tedious_1.Request(query, (reqErr) => {
                    conn.close();
                    if (reqErr)
                        return reject(reqErr);
                    resolve(rows);
                });
                request.on('row', (columns) => {
                    const rowObj = {};
                    columns.forEach((col) => {
                        rowObj[col.metadata.colName] = col.value;
                    });
                    rows.push(rowObj);
                });
                conn.execSql(request);
            });
            conn.connect();
        });
    }
    async testConnection(connection) {
        try {
            await this.executeQuery(connection, 'SELECT 1 as ok');
            return true;
        }
        catch {
            return false;
        }
    }
    async getDatabases(connection) {
        const rows = await this.executeQuery(connection, "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');", 'master');
        return rows.map(r => r.name);
    }
    async getTables(connection, database) {
        const rows = await this.executeQuery(connection, "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';", database);
        return rows.map(r => ({ name: r.TABLE_NAME }));
    }
    async getTableSchema(connection, database, table) {
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
    async getTableData(connection, database, table, page, pageSize) {
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
    getBackupCommand(connection, database, tables, outputPath) {
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
    getRestoreCommand(connection, database, inputPath) {
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
exports.SqlServerProvider = SqlServerProvider;
