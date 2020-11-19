using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SqlStreamStore;


namespace EventServe.SqlStreamStore.MsSql
{
    public interface IMsSqlStreamStoreSettingsProvider
    {
        Task<MsSqlStreamStoreV3Settings> GetSettings();
    }

    public class MsSqlStreamStoreSettingsProvider: IMsSqlStreamStoreSettingsProvider
    {
        private readonly string _connectionString;
        private readonly string _schemaName;

        public MsSqlStreamStoreSettingsProvider(string connectionString, string schemaName)
        {
            _connectionString = connectionString;
            _schemaName = schemaName;
        }

        public Task<MsSqlStreamStoreV3Settings> GetSettings()
        {
            return Task.FromResult(new MsSqlStreamStoreV3Settings(_connectionString)
            {
                Schema = _schemaName
            });
        }
    }
}
