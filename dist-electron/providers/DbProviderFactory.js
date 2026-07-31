"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DbProviderFactory = void 0;
const MySqlProvider_1 = require("./MySqlProvider");
const PostgreSqlProvider_1 = require("./PostgreSqlProvider");
const SqlServerProvider_1 = require("./SqlServerProvider");
const MariaDbProvider_1 = require("./MariaDbProvider");
const SqliteProvider_1 = require("./SqliteProvider");
const OracleProvider_1 = require("./OracleProvider");
class DbProviderFactory {
    providers = new Map();
    constructor(cryptoService) {
        const mysql = new MySqlProvider_1.MySqlProvider(cryptoService);
        const pg = new PostgreSqlProvider_1.PostgreSqlProvider(cryptoService);
        const sqlserver = new SqlServerProvider_1.SqlServerProvider(cryptoService);
        const mariadb = new MariaDbProvider_1.MariaDbProvider(cryptoService);
        const sqlite = new SqliteProvider_1.SqliteProvider();
        const oracle = new OracleProvider_1.OracleProvider(cryptoService);
        this.providers.set('MYSQL', mysql);
        this.providers.set('POSTGRESQL', pg);
        this.providers.set('SQLSERVER', sqlserver);
        this.providers.set('MARIADB', mariadb);
        this.providers.set('SQLITE', sqlite);
        this.providers.set('ORACLE', oracle);
    }
    getProvider(dbType) {
        const key = (dbType || 'MYSQL').toUpperCase();
        const provider = this.providers.get(key);
        if (!provider) {
            throw new Error(`Unsupported database type: ${dbType}`);
        }
        return provider;
    }
}
exports.DbProviderFactory = DbProviderFactory;
