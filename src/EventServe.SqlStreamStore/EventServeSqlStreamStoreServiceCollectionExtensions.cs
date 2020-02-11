using EventServe.Services;
using EventServe.SqlStreamStore.Subscriptions;
using EventServe.Subscriptions;
using Microsoft.Extensions.DependencyInjection;

namespace EventServe.SqlStreamStore
{
    public static class EventServeSqlStreamStoreServiceCollectionExtensions
    {
        public static void AddEventServeSqlStreamStore(this IServiceCollection services)
        {
            services.AddTransient<IEventSerializer, SqlStreamStoreEventSerializer>();
            services.AddTransient<IEventStreamReader, SqlStreamStoreStreamReader>();
            services.AddTransient<IEventStreamWriter, SqlStreamStoreStreamWriter>();
            services.AddTransient<ITransientStreamSubscriptionConnection, SqlStreamStoreTransientSubscriptionConnection>();
            services.AddTransient<IPersistentStreamSubscriptionConnection, SqlStreamStorePersistentSubscriptionConnection>();
            services.AddTransient<IPersistentSubscriptionPositionManager, PersistentSubscriptionPositionManager>();
        }
    }
}
