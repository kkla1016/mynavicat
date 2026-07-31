"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MySqlProvider = void 0;
const promise_1 = __importDefault(require("mysql2/promise"));
class MySqlProvider {
    dbType = 'MySQL';
    cryptoService;
    constructor(cryptoService) {
        this.cryptoService = cryptoService;
    }
    getConnConfig(connection, databaseOverride) {
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
    async testConnection(connection) {
        try {
            const conn = await promise_1.default.createConnection(this.getConnConfig(connection));
            await conn.ping();
            await conn.end();
            return true;
        }
        catch {
            return false;
        }
    }
    async getDatabases(connection) {
        const conn = await promise_1.default.createConnection(this.getConnConfig(connection));
        const [rows] = await conn.query("SHOW DATABASES WHERE `Database` NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys');");
        await conn.end();
        return rows.map((r) => r.Database);
    }
    async getTables(connection, database) {
        const conn = await promise_1.default.createConnection(this.getConnConfig(connection, database));
        const [rows] = await conn.query('SHOW TABLES;');
        await conn.end();
        const key = Object.keys(rows[0] || {})[0];
        return rows.map((r) => ({ name: r[key] }));
    }
    async getTableSchema(connection, database, table) {
        const conn = await promise_1.default.createConnection(this.getConnConfig(connection, database));
        const [rows] = await conn.query(`DESCRIBE \`${table}\`;`);
        await conn.end();
        return {
            tableName: table,
            columns: rows.map((r) => ({
                name: r.Field,
                dataType: r.Type,
                isNullable: r.Null === 'YES',
                isPrimaryKey: r.Key === 'PRI'
            }))
        };
    }
    async getTableData(connection, database, table, page, pageSize) {
        const conn = await promise_1.default.createConnection(this.getConnConfig(connection, database));
        const offset = (page - 1) * pageSize;
        const [countRows] = await conn.query(`SELECT COUNT(*) as total FROM \`${table}\`;`);
        const totalCount = countRows[0].total;
        const [rows] = await conn.query(`SELECT * FROM \`${table}\` LIMIT ? OFFSET ?;`, [pageSize, offset]);
        await conn.end();
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
    getRestoreCommand(connection, database, inputPath) {
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
exports.MySqlProvider = MySqlProvider;
