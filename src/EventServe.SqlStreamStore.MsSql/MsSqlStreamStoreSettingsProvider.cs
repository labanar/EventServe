using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SqlStreamStore;


namespace EventServe.SqlStreamStore.MsSql
{
    public interface IMsSqlStreamStoreSettingsProvider
    {
        Task<MsSqlStreamStoreSettings> GetSettings();
    }

    public class MsSqlStreamStoreSettingsProvider: IMsSqlStreamStoreSettingsProvider
    {
        private readonly string _connectionString;

        public MsSqlStreamStoreSettingsProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<MsSqlStreamStoreSettings> GetSettings()
        {
            return Task.FromResult(new MsSqlStreamStoreSettings(_connectionString)
            {
                Schema = "TestSchema"
            });
        }
    }
}
