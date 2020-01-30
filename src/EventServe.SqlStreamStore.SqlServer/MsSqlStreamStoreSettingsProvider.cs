using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SqlStreamStore;


namespace EventServe.SqlStreamStore.SqlServer
{
    public interface IMsSqlStreamStoreSettingsProvider
    {
        Task<MsSqlStreamStoreSettings> GetSettings();
    }

    public class MsSqlStreamStoreSettingsProvider: IMsSqlStreamStoreSettingsProvider
    {
        private readonly MsSqlStreamStoreOptions _options;

        public MsSqlStreamStoreSettingsProvider(IOptions<MsSqlStreamStoreOptions> options)
        {
            _options = options.Value;
        }

        public Task<MsSqlStreamStoreSettings> GetSettings()
        {
            return Task.FromResult(new MsSqlStreamStoreSettings(_options.ConnectionString)
            {
                Schema = _options.SchemaName
            });
        }
    }
}
