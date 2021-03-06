﻿using SqlStreamStore;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.MsSql
{
    public class MsSqlStreamStoreProvider : ISqlStreamStoreProvider, ISqlStreamStoreSubscriptionStoreProvider
    {
        private readonly IMsSqlStreamStoreSettingsProvider _msSqlStreamStoreSettings;

        public MsSqlStreamStoreProvider(IMsSqlStreamStoreSettingsProvider msSqlStreamStoreSettings)
        {
            _msSqlStreamStoreSettings = msSqlStreamStoreSettings;
        }

        public async Task<IStreamStore> GetStreamStore()
        {
            var settings =  await _msSqlStreamStoreSettings.GetSettings();
            return new MsSqlStreamStoreV3(settings);
        }
    }

    public class MsSqlStreamStoreSubscriptionStoreProvider : ISqlStreamStoreProvider, ISqlStreamStoreSubscriptionStoreProvider
    {
        private readonly IMsSqlStreamStoreSettingsProvider _msSqlStreamStoreSettings;

        public MsSqlStreamStoreSubscriptionStoreProvider(IMsSqlStreamStoreSettingsProvider msSqlStreamStoreSettings)
        {
            _msSqlStreamStoreSettings = msSqlStreamStoreSettings;
        }

        public async Task<IStreamStore> GetStreamStore()
        {
            var settings = await _msSqlStreamStoreSettings.GetSettings();
            settings.Schema = "Subscriptions";
            return new MsSqlStreamStoreV3(settings);
        }
    }
}
