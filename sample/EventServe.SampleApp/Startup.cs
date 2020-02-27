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



            var productRepo = app.ApplicationServices.GetRequiredService<IEventRepository<Product>>();
            var subscriptionManager = app.ApplicationServices.GetRequiredService<ISubscriptionManager>();

            //for(int i = 0; i <= 20; i++)
            //    await CreateProduct(productRepo);

            //await ResetSubscription(subscriptionManager, Guid.Parse("dff350a8-92df-4cd9-9ef8-23ba57ded611"));
            //SimulatePriceFluctuations(app.ApplicationServices,
            //                          1000,
            //                          Guid.Parse("5BF32D06-10C6-4ECA-AD3E-0FBB3B73C9A1"),
            //                          Guid.Parse("177C7086-F845-446F-878E-14912F64BBBE"),
            //                          Guid.Parse("43BA0692-A3E5-43DC-A8D9-2AA3FF1C21BF"),
            //                          Guid.Parse("1747757E-B5A8-4E4E-AC15-2CC2A43E3BD1"),
            //                          Guid.Parse("EFEE5AB2-FD4A-49FC-8902-46E07D867519"),
            //                          Guid.Parse("3B4F01F4-9FFF-40B0-9DBB-4F4F9877F50B"),
            //                          Guid.Parse("55044E18-D61C-45C2-80F8-57C58410E874"),
            //                          Guid.Parse("86F305C8-8B12-4022-B89E-63BDA626B508"),
            //                          Guid.Parse("7C420F1E-E9A4-4644-8E9B-6CEC8F6C8105"),
            //                          Guid.Parse("675184E6-0671-47B4-BC1F-796CD824C34A"),
            //                          Guid.Parse("8A7A0A2A-3C22-4487-8FD6-89AC5D9A897D"),
            //                          Guid.Parse("E76D6114-B299-4530-ACAD-A5B0C638F1A3"),
            //                          Guid.Parse("50A0A180-FB7D-4DBD-A62A-AA21F8F59ED1"),
            //                          Guid.Parse("59781D29-CD2C-42E3-A6AD-B5C9DAE76E08"),
            //                          Guid.Parse("DA55DFF0-E0DC-4A99-BB8A-B7586299999A"),
            //                          Guid.Parse("B1258FCE-2E74-4D3A-A539-C8747CE3D61B"),
            //                          Guid.Parse("F6584AB2-80FD-410C-8DC7-E365A525C774"),
            //                          Guid.Parse("815EF0EB-E1BD-4B72-B0BA-E4EC6D42852D"),
            //                          Guid.Parse("08280498-935A-4060-BE33-E9446DFFDB28"),
            //                          Guid.Parse("9DA61C8B-A605-481D-B5B8-F57777D8FFC2"),
            //                          Guid.Parse("DEEEB648-273F-4DDC-88E4-FDA60D58DA09"));

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


        private async Task SimulatePriceFluctuations(IServiceProvider serviceProvider, int iterations, params Guid[] productIds)
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

                    using var scope = serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetService<IEventRepository<Product>>();

                    var product = await repository.GetById(productId);
                    product.ResetProductPrice(newPrice, "CAD");
                    await repository.SaveAsync(product);
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
