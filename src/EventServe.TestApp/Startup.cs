using System;
using EventServe.SqlStreamStore.MsSql.DependencyInjection;
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
            Configuration["ConnectionStrings:MsSqlStreamStoreDb"],
            new Assembly[] { 
                typeof(Startup).Assembly 
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseEventServe();
            var manager = app.ApplicationServices.GetRequiredService<ISubscriptionRootManager>();

            var aggregateId = Guid.Parse("176a5024-6305-4f54-a2ce-e004bd62a118");
            var streamId =
                StreamIdBuilder.Create()
                .WithAggregateId(Guid.Parse("176a5024-6305-4f54-a2ce-e004bd62a118"))
                .WithAggregateType<DummyAggregate>()
                .Build();
            var random = new Random();


            var subscriptionRootManagerStreamId =
                StreamIdBuilder.Create()
                .WithAggregateId(Guid.Empty)
                .WithAggregateType<SubscriptionManagerRoot>()
                .Build();

            var streamWriter = app.ApplicationServices.GetRequiredService<IEventStreamWriter>();
            //await CreateStreamData(aggregateId, streamId, streamWriter);

            //var builder = app.ApplicationServices.GetRequiredService<ITransientSubscriptionBuilder<MyStreamSubscription>>();
            //var subscription = builder
            //    .SubscribeToAggregateCategory<DummyAggregate>()
            //    .ListenFor<DummyNameChangedEvent>()
            //    .ListenFor<DummyUrlChangedEvent>()
            //    .ListenFor<DummyCreatedEvent>()
            //    .StartAtBeginningOfStream()
            //    .Build();

            var builder = app.ApplicationServices.GetRequiredService<IPersistentSubscriptionBuilder<MyStreamSubscription>>();
            var subscription = builder
                .SubscribeToAggregateCategory<DummyAggregate>()
                .ListenFor<DummyNameChangedEvent>()
                .ListenFor<DummyUrlChangedEvent>()
                .ListenFor<DummyCreatedEvent>()
                .Build();

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
