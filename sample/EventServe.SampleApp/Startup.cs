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
using System.Reflection;
using System.Threading.Tasks;
using EventServe.Services;
using EventServe.Subscriptions.Persistent;
using EventServe.SampleApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using EventServe.Projections;
using EventServe.SampleApp.Domain;
using EventServe.EventStore;
using EventServe.Extensions.Microsoft.DependencyInjection;

namespace EventServe.SampleApp
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

            services.AddDbContextPool<SampleContext>(options =>
            {
                options.UseSqlServer(Configuration["ConnectionStrings:ReadModelDb"]);
            });
            services.AddTransient<IPartitionedProjectionStateRepository, PartitionedProjectionStateRepository>();

            services.AddEventServe(options =>
            {
                var connOptions = Configuration.GetSection("EventStoreConnectionOptions").Get<EventStoreConnectionOptions>();
                options.Host = connOptions.Host;
                options.Port = connOptions.Port;
                options.Username = connOptions.Username;
                options.Password = connOptions.Password;
            },
            new Assembly[] {
                typeof(Startup).Assembly ,
                typeof(PersistentSubscriptionProfile).Assembly
            });

            //services.AddEventServe(options =>
            //{
            //    options.ConnectionString = Configuration["ConnectionStrings:MsSqlStreamStoreDb"];
            //    options.SchemaName = Configuration["MsSqlStreamStoreOptions:SchemaName"];
            //},
            //Configuration["ConnectionStrings:MsSqlStreamStoreDb"],
            //new Assembly[] {
            //    typeof(PersistentSubscriptionProfile).Assembly,
            //    typeof(Startup).Assembly,
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseEventServe();
            //app.UseEventServeMsSqlStreamStore();



            var productRepo = app.ApplicationServices.GetRequiredService<IEventRepository<Product>>();
            var subscriptionManager = app.ApplicationServices.GetRequiredService<ISubscriptionManager>();
            //await CreateProduct(productRepo);
            await ResetSubscription(subscriptionManager, Guid.Parse("554cb735-99f2-4f75-91de-dd13ef5478ac"));
            //SimulatePriceFluctuations(productRepo, 10000, Guid.Parse("a3b200a4-ae13-4c3b-afcb-7452f4bcdbcf"), Guid.Parse("a31d48fa-9e70-454b-a7c2-2bed8137bcf8"));

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private async Task CreateProduct(IEventRepository<Product> productRepository)
        {
            var productId = Guid.NewGuid();
            var product = new Product(new Domain.Commands.ResetProductCommand
            {
                ProductId = productId,
                Name = $"Product Name {productId}",
                Available = true,
                Url = "example.com",
                CurrencyCode = "CAD",
                Price = 129.99,
            });

            await productRepository.SaveAsync(product);
        }


        private async Task SimulatePriceFluctuations(IEventRepository<Product> productRepository, int iterations, params Guid[] productIds)
        {
            for(int i = 0; i < iterations; i++)
            {
                var rand = new Random();

                var maxPrice = 129.99;
                var fluctuationRange = maxPrice / 2;

                foreach (var productId in productIds)
                {
                    var sign = rand.Next(0, 2);
                    var fluctuationAmount = fluctuationRange * rand.NextDouble();
                    var newPrice = (sign == 0) ? maxPrice - fluctuationAmount : maxPrice + fluctuationAmount;

                    var product = await productRepository.GetById(productId);
                    product.ResetProductPrice(newPrice, "CAD");
                    await productRepository.SaveAsync(product);
                }

                //await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }

        private async Task ResetSubscription(ISubscriptionManager manager, Guid subscriptionId)
        {
            await manager.Reset(subscriptionId);
        }
    }
}
