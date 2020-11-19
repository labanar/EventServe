using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Threading.Tasks;
using System.Reflection;
using EventServe.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventServe.SqlStreamStore.MsSql.Extensions.Microsoft.DependencyInjection
{
    public static class EventServeSqlStreamStoreServiceCollectionExtensions
    {
        public static void AddEventServe(this IServiceCollection services, Action<MsSqlStreamStoreOptions> setupAction, Assembly[] assemblies)
        {
            services.AddEventServeCore(assemblies);
            services.AddEventServeSqlStreamStore();
            services.Configure(setupAction);
            services.AddTransient<IMsSqlStreamStoreSettingsProvider>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<MsSqlStreamStoreOptions>>();
                return new MsSqlStreamStoreSettingsProvider(options.Value.ConnectionString, options.Value.SchemaName);
            });
            services.AddDbContextPool<SqlStreamStoreContext>((serviceProvider, options) =>
            {
                var sqlStreamStoreOptions = serviceProvider.GetRequiredService<IOptions<MsSqlStreamStoreOptions>>();
                options.UseSqlServer(sqlStreamStoreOptions.Value.ConnectionString, sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(typeof(MsSqlStreamStoreOptions).Assembly.FullName);
                });
            });
            services.AddTransient<ISqlStreamStoreSubscriptionStoreProvider, MsSqlStreamStoreSubscriptionStoreProvider>();
            services.AddTransient<ISqlStreamStoreProvider, MsSqlStreamStoreProvider>();
        }

        public static void UseEventServeMsSqlStreamStore(this IApplicationBuilder applicationBuilder)
        {

            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SqlStreamStoreContext>();
                context.Database.Migrate();
            }

            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<IMsSqlStreamStoreSettingsProvider>();

                var settings = settingsProvider.GetSettings();
                Task.WaitAll(settings);

                var store = new MsSqlStreamStoreV3(settings.Result);
                store.CreateSchemaIfNotExists().Wait();
                var checkResult = store.CheckSchema();
                Task.WaitAll(checkResult);
            }


            applicationBuilder.RegisterEventServeSubscriptions();
        }

    }
}
