using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNavicat.Api.Providers
{
    public interface IDbProviderFactory
    {
        IDbProvider GetProvider(string dbType);
    }

    /// <summary>
    /// 資料庫 Provider 工廠類別
    /// </summary>
    public class DbProviderFactory : IDbProviderFactory
    {
        private readonly Dictionary<string, IDbProvider> _providers;

        public DbProviderFactory(IEnumerable<IDbProvider> providers)
        {
            _providers = providers.ToDictionary(p => p.DbType, StringComparer.OrdinalIgnoreCase);
        }

        public IDbProvider GetProvider(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType))
                throw new ArgumentException("Database type cannot be null or empty", nameof(dbType));

            if (_providers.TryGetValue(dbType, out var provider))
            {
                return provider;
            }

            throw new NotSupportedException($"Database type '{dbType}' is not supported.");
        }
    }
}
