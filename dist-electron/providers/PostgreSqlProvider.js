"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.PostgreSqlProvider = void 0;
const pg_1 = require("pg");
class PostgreSqlProvider {
    dbType = 'PostgreSQL';
    cryptoService;
    constructor(cryptoService) {
        this.cryptoService = cryptoService;
    }
    getClient(connection, databaseOverride) {
        const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
        return new pg_1.Client({
            host: connection.Host || connection.host,
            port: (connection.Port || connection.port) || 5432,
            user: connection.Username || connection.username,
            password: rawPassword,
            database: databaseOverride || (connection.DatabaseName || connection.databaseName) || 'postgres',
            connectionTimeoutMillis: 10000
        });
    }
    async testConnection(connection) {
        try {
            const client = this.getClient(connection);
            await client.connect();
            await client.end();
            return true;
        }
        catch {
            return false;
        }
    }
    async getDatabases(connection) {
        const client = this.getClient(connection, 'postgres');
        await client.connect();
        const res = await client.query("SELECT datname FROM pg_database WHERE datistemplate = false AND datname NOT IN ('postgres');");
        await client.end();
        return res.rows.map(r => r.datname);
    }
    async getTables(connection, database) {
        const client = this.getClient(connection, database);
        await client.connect();
        const res = await client.query("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';");
        await client.end();
        return res.rows.map(r => ({ name: r.table_name }));
    }
    async getTableSchema(connection, database, table) {
        const client = this.getClient(connection, database);
        await client.connect();
        const res = await client.query(`
      SELECT column_name, data_type, is_nullable
      FROM information_schema.columns
      WHERE table_schema = 'public' AND table_name = $1
      ORDER BY ordinal_position;
    `, [table]);
        await client.end();
        return {
            tableName: table,
            columns: res.rows.map(r => ({
                name: r.column_name,
                dataType: r.data_type,
                isNullable: r.is_nullable === 'YES'
            }))
        };
    }
    async getTableData(connection, database, table, page, pageSize) {
        const client = this.getClient(connection, database);
        await client.connect();
        const offset = (page - 1) * pageSize;
        const countRes = await client.query(`SELECT COUNT(*) as total FROM "${table}";`);
        const totalCount = parseInt(countRes.rows[0].total, 10);
        const dataRes = await client.query(`SELECT * FROM "${table}" LIMIT $1 OFFSET $2;`, [pageSize, offset]);
        await client.end();
        return {
            items: dataRes.rows,
            page,
            pageSize,
            totalCount
        };
    }
    getBackupCommand(connection, database, tables, outputPath) {
        const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
        const host = connection.Host || connection.host;
        const port = (connection.Port || connection.port) || 5432;
        const user = connection.Username || connection.username;
        let args = `-h "${host}" -p ${port} -U "${user}" -F p -f "${outputPath}"`;
        if (tables && tables.length > 0) {
            args += ' ' + tables.map(t => `-t "${t}"`).join(' ');
        }
        args += ` "${database}"`;
        return {
            executable: 'pg_dump',
            arguments: args,
            environmentVariables: rawPassword ? { PGPASSWORD: rawPassword } : undefined,
            outputFilePath: outputPath
        };
    }
    getRestoreCommand(connection, database, inputPath) {
        const rawPassword = this.cryptoService.decrypt(connection.EncryptedPassword || connection.encryptedPassword);
        const host = connection.Host || connection.host;
        const port = (connection.Port || connection.port) || 5432;
        const user = connection.Username || connection.username;
        const args = `-h "${host}" -p ${port} -U "${user}" -d "${database}" -f "${inputPath}"`;
        return {
            executable: 'psql',
            arguments: args,
            environmentVariables: rawPassword ? { PGPASSWORD: rawPassword } : undefined,
            outputFilePath: inputPath
        };
    }
}
exports.PostgreSqlProvider = PostgreSqlProvider;
