using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.SqlServer
{
    public static class MsSqlStreamStoreExtensions
    {
        public static void MigrateMsSqlStreamStore(this IApplicationBuilder applicationBuilder)
        {
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<IMsSqlStreamStoreSettingsProvider>();

                var settings = settingsProvider.GetSettings();
                Task.WaitAll(settings);

                var store = new MsSqlStreamStore(settings.Result);
                var checkResult = store.CheckSchema();
                Task.WaitAll(checkResult);

                if (checkResult.Result.CurrentVersion != checkResult.Result.ExpectedVersion)
                {
                    store.CreateSchema();
                }
            }
        }
    }
}
