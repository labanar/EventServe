using EventServe.EventStore.Interfaces;
using EventServe.EventStore.Subscriptions;
using EventServe.Services;
using EventServe.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventServe;
using System.Reflection;

namespace EventServe.EventStore.DependencyInjection
{
    public static class EventServeEventStoreServiceCollectionExtension
    {
        public static void AddEventServe(this IServiceCollection services, Action<EventStoreConnectionOptions> setupAction, Assembly[] assemblies)
        {
            services.UseEventServeCore(assemblies);
            services.Configure(setupAction);
            services.AddTransient<IEventStoreConnectionProvider, EventStoreConnectionProvider>();
            services.AddTransient<IEventStreamReader, EventStoreStreamReader>();
            services.AddTransient<IEventStreamWriter, EventStoreStreamWriter>();
            services.AddTransient<IEventSerializer, EventSerializer>();
            services.AddTransient<IPersistentStreamSubscription, EventStorePersistentSubscription>();
        }
    }
}
