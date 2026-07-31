"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MariaDbProvider = void 0;
const MySqlProvider_1 = require("./MySqlProvider");
class MariaDbProvider {
    dbType = 'MariaDB';
    innerProvider;
    constructor(cryptoService) {
        this.innerProvider = new MySqlProvider_1.MySqlProvider(cryptoService);
    }
    testConnection(connection) {
        return this.innerProvider.testConnection(connection);
    }
    getDatabases(connection) {
        return this.innerProvider.getDatabases(connection);
    }
    getTables(connection, database) {
        return this.innerProvider.getTables(connection, database);
    }
    getTableSchema(connection, database, table) {
        return this.innerProvider.getTableSchema(connection, database, table);
    }
    getTableData(connection, database, table, page, pageSize) {
        return this.innerProvider.getTableData(connection, database, table, page, pageSize);
    }
    getBackupCommand(connection, database, tables, outputPath) {
        const cmd = this.innerProvider.getBackupCommand(connection, database, tables, outputPath);
        cmd.executable = 'mariadb-dump';
        return cmd;
    }
    getRestoreCommand(connection, database, inputPath) {
        const cmd = this.innerProvider.getRestoreCommand(connection, database, inputPath);
        cmd.executable = 'mariadb';
        return cmd;
    }
}
exports.MariaDbProvider = MariaDbProvider;
