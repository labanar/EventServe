using SqlStreamStore;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.SqlServer
{
    public class MsSqlStreamStoreProvider : ISqlStreamStoreProvider
    {
        private readonly IMsSqlStreamStoreSettingsProvider _msSqlStreamStoreSettings;

        public MsSqlStreamStoreProvider(IMsSqlStreamStoreSettingsProvider msSqlStreamStoreSettings)
        {
            _msSqlStreamStoreSettings = msSqlStreamStoreSettings;
        }

        public async Task<IStreamStore> GetStreamStore()
        {
            var settings =  await _msSqlStreamStoreSettings.GetSettings();
            return new MsSqlStreamStore(settings);
        }
    }
}
