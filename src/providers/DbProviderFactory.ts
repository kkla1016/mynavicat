import { IDbProvider } from './IDbProvider';
import { MySqlProvider } from './MySqlProvider';
import { PostgreSqlProvider } from './PostgreSqlProvider';
import { SqlServerProvider } from './SqlServerProvider';
import { MariaDbProvider } from './MariaDbProvider';
import { SqliteProvider } from './SqliteProvider';
import { OracleProvider } from './OracleProvider';
import { CryptoService } from '../services/cryptoService';

export class DbProviderFactory {
  private providers: Map<string, IDbProvider> = new Map();

  constructor(cryptoService: CryptoService) {
    const mysql = new MySqlProvider(cryptoService);
    const pg = new PostgreSqlProvider(cryptoService);
    const sqlserver = new SqlServerProvider(cryptoService);
    const mariadb = new MariaDbProvider(cryptoService);
    const sqlite = new SqliteProvider();
    const oracle = new OracleProvider(cryptoService);

    this.providers.set('MYSQL', mysql);
    this.providers.set('POSTGRESQL', pg);
    this.providers.set('SQLSERVER', sqlserver);
    this.providers.set('MARIADB', mariadb);
    this.providers.set('SQLITE', sqlite);
    this.providers.set('ORACLE', oracle);
  }

  public getProvider(dbType: string): IDbProvider {
    const key = (dbType || 'MYSQL').toUpperCase();
    const provider = this.providers.get(key);
    if (!provider) {
      throw new Error(`Unsupported database type: ${dbType}`);
    }
    return provider;
  }
}
