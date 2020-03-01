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

        public MsSqlStreamStoreSettingsProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<MsSqlStreamStoreV3Settings> GetSettings()
        {
            return Task.FromResult(new MsSqlStreamStoreV3Settings(_connectionString)
            {
                Schema = "TestSchema"
            });
        }
    }
}
