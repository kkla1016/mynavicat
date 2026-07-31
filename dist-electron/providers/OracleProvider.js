"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.OracleProvider = void 0;
class OracleProvider {
    dbType = 'Oracle';
    cryptoService;
    constructor(cryptoService) {
        this.cryptoService = cryptoService;
    }
    testConnection(connection) {
        return Promise.resolve(true);
    }
    getDatabases(connection) {
        const sid = connection.DatabaseName || connection.databaseName || 'XE';
        return Promise.resolve([sid]);
    }
    getTables(connection, database) {
        return Promise.resolve([]);
    }
    getTableSchema(connection, database, table) {
        return Promise.resolve({ tableName: table, columns: [] });
    }
    getTableData(connection, database, table, page, pageSize) {
        return Promise.resolve({ items: [], page: 1, pageSize: 20, totalCount: 0 });
    }
    getBackupCommand(connection, database, tables, outputPath) {
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
    getRestoreCommand(connection, database, inputPath) {
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
exports.OracleProvider = OracleProvider;
