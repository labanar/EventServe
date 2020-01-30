using EventServe.Services;
using EventServe.SqlStreamStore.SqlServer;
using EventServe.SqlStreamStore.Subscriptions;
using EventServe.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventServe.SqlStreamStore.Extensions.DependencyInjection
{
    public static class EventServeSqlStreamStoreServiceCollectionExtensions
    {
        public static void AddEventServeMsSqlStreamStore(this IServiceCollection services, Action<MsSqlStreamStoreOptions> setupAction, string connectionString)
        {
            services.Configure(setupAction);
            services.AddDbContext<MsSqlStreamStoreContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            services.AddTransient<ISqlStreamStoreSubscriptionManager, MsSqlStreamStoreSubscriptionManager>();
            services.AddTransient<IMsSqlStreamStoreSettingsProvider, MsSqlStreamStoreSettingsProvider>();
            services.AddTransient<ISqlStreamStoreProvider, MsSqlStreamStoreProvider>();
            services.AddTransient<IEventSerializer, SqlStreamStoreEventSerializer>();
            services.AddTransient<IEventStreamReader, SqlStreamStoreStreamReader>();
            services.AddTransient<IEventStreamWriter, SqlStreamStoreStreamWriter>();
            services.AddTransient<IPersistentStreamSubscription, SqlStreamStorePersistentSubscription>();
        }

        public static void UseEventServeMsSqlStreamStore(this IApplicationBuilder app)
        {
            using(var scope = app.ApplicationServices.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<MsSqlStreamStoreContext>())
                {
                    context.Database.EnsureCreated();
                }
            }

            app.MigrateMsSqlStreamStore();
        }
    }
}
