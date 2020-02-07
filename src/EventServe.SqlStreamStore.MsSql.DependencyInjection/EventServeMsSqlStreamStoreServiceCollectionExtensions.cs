using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SqlStreamStore;
using System;
using System.Threading.Tasks;
using EventServe;
using System.Reflection;
using System.Linq;

namespace EventServe.SqlStreamStore.MsSql.DependencyInjection
{
    public static class EventServeSqlStreamStoreServiceCollectionExtensions
    {
        public static void AddEventServe(this IServiceCollection services, Action<MsSqlStreamStoreOptions> setupAction, string connectionString, Assembly[] assemblies)
        {
            services.RegisterAllTypes<EventServe.Subscriptions.IStreamSubscription>(assemblies, ServiceLifetime.Singleton);
            services.UseEventServeCore(assemblies);
            services.AddEventServeSqlStreamStore();
            services.Configure(setupAction);
            services.AddTransient<IMsSqlStreamStoreSettingsProvider>(_ => new MsSqlStreamStoreSettingsProvider(connectionString));
            services.AddDbContext<SqlStreamStoreContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(typeof(MsSqlStreamStoreOptions).Assembly.FullName);
                });
            });
            services.AddTransient<ISqlStreamStoreSubscriptionStoreProvider, MsSqlStreamStoreSubscriptionStoreProvider>();
            services.AddTransient<ISqlStreamStoreProvider, MsSqlStreamStoreProvider>();
        }

        public static void RegisterAllTypes<T>(this IServiceCollection services, Assembly[] assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
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
                    store.CreateSchema().Wait();
            }


            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SqlStreamStoreContext>();
                context.Database.EnsureCreated();
                context.Database.Migrate();
            }


            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var subscriptions = scope.ServiceProvider.GetServices<EventServe.Subscriptions.IStreamSubscription>();
            }
        }

    }
}
