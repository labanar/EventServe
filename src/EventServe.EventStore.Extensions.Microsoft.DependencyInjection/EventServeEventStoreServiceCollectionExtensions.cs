using EventServe.EventStore.Interfaces;
using EventServe.EventStore.Subscriptions;
using EventServe.Services;
using EventServe.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using EventServe.Extensions.Microsoft.DependencyInjection;

namespace EventServe.EventStore.Extensions.Microsoft.DepdendencyInjection
{
    public static class EventServeEventStoreServiceCollectionExtensions
    {
        public static void AddEventServe(this IServiceCollection services, Action<EventStoreConnectionOptions> setupAction, Assembly[] assemblies)
        {
            services.AddEventServeCore(assemblies);
            services.Configure(setupAction);
            services.AddTransient<IEventStoreConnectionProvider, EventStoreConnectionProvider>();
            services.AddTransient<IEventStreamReader, EventStoreStreamReader>();
            services.AddTransient<IEventStreamWriter, EventStoreStreamWriter>();
            services.AddTransient<IEventSerializer, EventSerializer>();
            services.AddTransient<IPersistentStreamSubscription, EventStorePersistentSubscription>();
        }
    }
}
