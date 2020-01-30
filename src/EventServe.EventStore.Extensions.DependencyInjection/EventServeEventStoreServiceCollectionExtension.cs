using EventServe.EventStore.Interfaces;
using EventServe.EventStore.Subscriptions;
using EventServe.Services;
using EventServe.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventServe.EventStore.Extensions.DependencyInjection
{
    public static class EventServeEventStoreServiceCollectionExtension
    {
        public static void AddEventServeEventStore(this IServiceCollection services, Action<EventStoreConnectionOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddTransient<IEventStoreConnectionProvider, EventStoreConnectionProvider>();
            services.AddTransient<IEventStreamReader, EventStoreStreamReader>();
            services.AddTransient<IEventStreamWriter, EventStoreStreamWriter>();
            services.AddTransient<IEventSerializer, EventSerializer>();
            services.AddTransient<IPersistentSreamSubscription, EventStorePersistentSubscription>();
        }
    }
}
