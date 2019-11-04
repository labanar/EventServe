using EventServe.EventStore.Interfaces;
using EventServe.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventServe.EventStore.Extensions.DependencyInjection
{
    public static class EventServeEventStoreServiceCollectionExtension
    {
        public static void AddEventServeEventStore(this IServiceCollection services, Action<EventStoreConnectionOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddScoped<IEventStoreConnectionProvider, EventStoreConnectionProvider>();
            services.AddScoped<IEventStreamReader, EventStoreStreamReader>();
            services.AddScoped<IEventStreamWriter, EventStoreStreamWriter>();
            services.AddScoped<IEventSerializer, EventSerializer>();
        }
    }
}
