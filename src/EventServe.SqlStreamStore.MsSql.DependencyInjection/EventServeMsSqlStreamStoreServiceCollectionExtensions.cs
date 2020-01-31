using EventServe.Services;
using EventServe.SqlStreamStore.MsSql;
using EventServe.SqlStreamStore.Subscriptions;
using EventServe.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.MsSql.DependencyInjection
{
    public static class EventServeSqlStreamStoreServiceCollectionExtensions
    {
        private static void AddEventServeSqlStreamStore(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IEventSerializer, SqlStreamStoreEventSerializer>();
            services.AddTransient<IEventStreamReader, SqlStreamStoreStreamReader>();
            services.AddTransient<IEventStreamWriter, SqlStreamStoreStreamWriter>();
            services.AddTransient(typeof(IEventRepository<>), typeof(EventRepository<>));
            services.AddTransient<IPersistentStreamSubscription, SqlStreamStorePersistentSubscription>();
            services.AddTransient<SqlStreamStoreSubscriptionManager, SqlStreamStoreSubscriptionManager>();
        }


        public static void AddEventServe(this IServiceCollection services, Action<MsSqlStreamStoreOptions> setupAction, string connectionString)
        {
            services.AddEventServeSqlStreamStore(connectionString);
            services.Configure(setupAction);
            services.AddTransient<IMsSqlStreamStoreSettingsProvider, MsSqlStreamStoreSettingsProvider>();
            services.AddTransient<ISqlStreamStoreSubscriptionStoreProvider, MsSqlStreamStoreSubscriptionStoreProvider>();
            services.AddTransient<ISqlStreamStoreProvider, MsSqlStreamStoreProvider>();
        }

        public static void UseEventServe(this IApplicationBuilder applicationBuilder)
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


                var subscriptionStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(settings.Result.ConnectionString)
                {
                    Schema = "Subscriptions"
                });
                checkResult = subscriptionStore.CheckSchema();
                if (checkResult.Result.CurrentVersion != checkResult.Result.ExpectedVersion)
                {
                    subscriptionStore.CreateSchema();
                }
            }
        }

    }
}
