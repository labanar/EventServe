using EventServe.Services;
using EventServe.SqlStreamStore.SqlServer;
using EventServe.SqlStreamStore.Subscriptions;
using EventServe.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddTransient<ISqlStreamStoreSubscriptionManager, SqlStreamStoreSubscriptionManager>();
        }


        public static void AddEventServe(this IServiceCollection services, Action<MsSqlStreamStoreOptions> setupAction, string connectionString)
        {
            services.AddEventServeSqlStreamStore(connectionString);
            services.Configure(setupAction);
            services.AddTransient<IMsSqlStreamStoreSettingsProvider, MsSqlStreamStoreSettingsProvider>();
            services.AddTransient<ISqlStreamStoreSubscriptionStoreProvider, MsSqlStreamStoreSubscriptionStoreProvider>();
            services.AddTransient<ISqlStreamStoreProvider, MsSqlStreamStoreProvider>();
        }


        public static void UseEventStore<MsSqlStreamStoreOptions>(this IApplicationBuilder app)
        {
            app.MigrateMsSqlStreamStore();
        }
    }
}
