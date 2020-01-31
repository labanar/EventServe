using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using EventServe.EventStore.Subscriptions;
using EventServe.Subscriptions;
using EventServe.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventServe.SqlStreamStore.MsSql.DependencyInjection;

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
            services.AddMediatR(typeof(Startup).Assembly, typeof(EventStorePersistentSubscription).Assembly, typeof(PersistentStreamSubscription).Assembly);
            services.AddControllers();

            //services.AddEventServe(options =>
            //{
            //    var connOptions = Configuration.GetSection("EventStoreConnectionOptions").Get<EventStoreConnectionOptions>();
            //    options.Host = connOptions.Host;
            //    options.Port = connOptions.Port;
            //    options.Username = connOptions.Username;
            //    options.Password = connOptions.Password;
            //});

            services.AddEventServe(options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:MsSqlStreamStoreDb"];
                options.SchemaName = Configuration["MsSqlStreamStoreOptions:SchemaName"];
            },
            Configuration["ConnectionStrings:MsSqlStreamStoreDb"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseEventServe();

            var sub1 = app.ApplicationServices.GetRequiredService<IPersistentStreamSubscription>();
            var sub2 = app.ApplicationServices.GetRequiredService<IPersistentStreamSubscription>();

            var aggregateId = Guid.Parse("176a5024-6305-4f54-a2ce-e004bd62a118");
            var streamId =
                StreamIdBuilder.Create()
                .WithAggregateId(Guid.Parse("176a5024-6305-4f54-a2ce-e004bd62a118"))
                .WithAggregateType<DummyAggregate>()
                .Build();
            var random = new Random();


            await Task.Factory.StartNew(async () =>
            {
                var writeEvents = new List<Event> {
                new DummyNameChangedEvent(aggregateId, $"The new name {random.Next(100,9999)}"),
                new DummyUrlChangedEvent(aggregateId, $"https://newurl{random.Next(100,9999)}.example.com")
                };

                await Task.Delay(TimeSpan.FromMinutes(2));
                var streamWriter = app.ApplicationServices.GetRequiredService<IEventStreamWriter>();
                await streamWriter.AppendEventsToStream(streamId, writeEvents);
            });

            await Task.Factory.StartNew(async () =>
            {
                var writeEvents = new List<Event> {
                new DummyNameChangedEvent(aggregateId, $"The new name {random.Next(100,9999)}"),
                new DummyUrlChangedEvent(aggregateId, $"https://newurl{random.Next(100,9999)}.example.com")
                };

                await Task.Delay(TimeSpan.FromMinutes(4));
                var streamWriter = app.ApplicationServices.GetRequiredService<IEventStreamWriter>();
                await streamWriter.AppendEventsToStream(streamId, writeEvents);
            });

            await Task.Factory.StartNew(async () =>
            {
                var writeEvents = new List<Event> {
                new DummyNameChangedEvent(aggregateId, $"The new name {random.Next(100,9999)}"),
                new DummyUrlChangedEvent(aggregateId, $"https://newurl{random.Next(100,9999)}.example.com")
                };

                await Task.Delay(TimeSpan.FromMinutes(8));
                var streamWriter = app.ApplicationServices.GetRequiredService<IEventStreamWriter>();
                await streamWriter.AppendEventsToStream(streamId, writeEvents);
            });


            await Task.Factory.StartNew(async () =>
            {
                var writeEvents = new List<Event> {
                new DummyNameChangedEvent(aggregateId, $"The new name {random.Next(100,9999)}"),
                new DummyUrlChangedEvent(aggregateId, $"https://newurl{random.Next(100,9999)}.example.com")
                };

                await Task.Delay(TimeSpan.FromMinutes(15));
                var streamWriter = app.ApplicationServices.GetRequiredService<IEventStreamWriter>();
                await streamWriter.AppendEventsToStream(streamId, writeEvents);
            });

            await sub1.ConnectAsync(streamId);
            await sub2.ConnectAsync(streamId);

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
