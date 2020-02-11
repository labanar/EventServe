using System;
using EventServe.SqlStreamStore.MsSql.Extensions.Microsoft.DependencyInjection;
using EventServe.EventStore.Extensions.Microsoft.DepdendencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EventServe.EventStore.Subscriptions;
using EventServe.Subscriptions;
using MediatR;
using EventServe.Subscriptions.Domain;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using EventServe.Services;
using EventServe.Subscriptions.Persistent;
using EventServe.EventStore;
using EventServe.Extensions.Microsoft.DependencyInjection;
using System.Diagnostics;

namespace EventServe.TestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup).Assembly, typeof(EventStorePersistentSubscriptionConnection).Assembly, typeof(PersistentStreamSubscriptionConnection).Assembly);
            services.AddControllers();

            //services.AddEventServe(options =>
            //{
            //    var connOptions = Configuration.GetSection("EventStoreConnectionOptions").Get<EventStoreConnectionOptions>();
            //    options.Host = connOptions.Host;
            //    options.Port = connOptions.Port;
            //    options.Username = connOptions.Username;
            //    options.Password = connOptions.Password;
            //},
            //new Assembly[] {
            //    typeof(Startup).Assembly ,
            //    typeof(PersistentSubscriptionProfile).Assembly
            //});

            services.AddEventServe(options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:MsSqlStreamStoreDb"];
                options.SchemaName = Configuration["MsSqlStreamStoreOptions:SchemaName"];
            },
            Configuration["ConnectionStrings:MsSqlStreamStoreDb"],
            new Assembly[] {
                typeof(PersistentSubscriptionProfile).Assembly,
                typeof(Startup).Assembly,
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseEventServe();
            app.UseEventServeMsSqlStreamStore();

            var aggregateId = Guid.Parse("176a5024-6305-4f54-a2ce-e004bd62a118");
            var streamId =
                StreamIdBuilder.Create()
                .WithAggregateId(Guid.Parse("176a5024-6305-4f54-a2ce-e004bd62a118"))
                .WithAggregateType<DummyAggregate>()
                .Build();

            var streamWriter = app.ApplicationServices.GetRequiredService<IEventStreamWriter>();
            var streamReader = app.ApplicationServices.GetRequiredService<IEventStreamReader>();
            //await CreateStreamData(aggregateId, streamId, streamWriter);

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private async Task CreateStreamData(Guid aggregateId, string streamId, IEventStreamWriter streamWriter)
        {
            var random = new Random();
            await Task.Factory.StartNew(async () =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    var writeEvents = new List<Event> {
                        new DummyNameChangedEvent(aggregateId, $"The new name {random.Next(100,9999)}"),
                        new DummyUrlChangedEvent(aggregateId, $"https://newurl{random.Next(100,9999)}.example.com")
                    };

                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                    await streamWriter.AppendEventsToStream(streamId, writeEvents);
                }
            });
        }
    }
}
