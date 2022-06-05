using EventServe.EventStore.Extensions.Microsoft.DepdendencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventServe.EventStore.Subscriptions;
using EventServe.Subscriptions;
using MediatR;
using System.Reflection;
using EventServe.Subscriptions.Persistent;
using EventServe.SampleApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using EventServe.Projections;
using EventServe.EventStore;
using EventServe.Extensions.Microsoft.DependencyInjection;
using EventServe.Services;
using EventServe.SampleApp.Domain;
using System;
using EventServe.SampleApp.Domain.Commands;
using System.Threading.Tasks;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(typeof(Program).Assembly, typeof(EventStorePersistentSubscriptionConnection).Assembly, typeof(PersistentStreamSubscriptionConnection).Assembly);
builder.Services.AddControllers();
builder.Services.AddDbContext<SampleContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ReadModelDb"]);
});
builder.Services.AddTransient(typeof(IPartitionedProjectionStateRepository<>), typeof(PartitionedProjectionStateRepository<>));
builder.Services
    .AddEventServe(options =>
    {
        var connOptions = builder.Configuration.GetSection("EventStoreConnectionOptions").Get<EventStoreConnectionOptions>();
        options.Host = connOptions.Host;
        options.Port = connOptions.Port;
        options.Username = connOptions.Username;
        options.Password = connOptions.Password;
        options.DisableTls = true;
    },
    new Assembly[] {
        typeof(Program).Assembly ,
        typeof(PersistentSubscriptionProfile).Assembly
    });

//builder.Services.AddEventServe(options =>
//{
//    options.ConnectionString = Configuration["ConnectionStrings:MsSqlStreamStoreDb"];
//    options.SchemaName = Configuration["MsSqlStreamStoreOptions:SchemaName"];
//},
//new Assembly[] {
//    typeof(PersistentSubscriptionProfile).Assembly,
//    typeof(Startup).Assembly,
//});

var app = builder.Build();
app.UseEventServe();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

Func<WebApplication, Task> MigrateReadModelDatabase = async (app) =>
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var context = scope.ServiceProvider.GetService<SampleContext>();
        await context.Database.MigrateAsync();
    }

};


Func<WebApplication, Task> SeedProducts = async (app) =>
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var productRepo = app.Services.GetRequiredService<IEventRepository<Product>>();
        for (int i = 0; i <= 50; i++)
        {
            var productId = Guid.NewGuid();
            var product = new Product(new ResetProductCommand
            {
                ProductId = productId,
                Name = $"Product Name {productId}",
                Available = true,
                Url = "example.com",
                CurrencyCode = "CAD",
                Price = 129.99,
            });

            await productRepo.SaveAsync(product);
        }
    }
};

Func<WebApplication, int, Task> SimulatePriceFluctuations = async (app, iterations) =>
{
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var repository = app.Services.GetRequiredService<IEventRepository<Product>>();
        using var context = scope.ServiceProvider.GetRequiredService<SampleContext>();
        var productIds = await context.Products.Select(x => x.ProductId).ToListAsync();

        for (int i = 0; i < iterations; i++)
        {
            var rand = new Random();

            var maxPrice = 129.99;
            var fluctuationRange = maxPrice / 2;

            foreach (var productId in productIds)
            {
                var coinToss = rand.Next(0, 10);

                if (coinToss < 8)
                    continue;

                var sign = rand.Next(0, 2);
                var fluctuationAmount = fluctuationRange * rand.NextDouble();
                var newPrice = (sign == 0) ? maxPrice - fluctuationAmount : maxPrice + fluctuationAmount;
                var product = await repository.GetById(productId);
                product.ResetProductPrice(newPrice, "CAD");
                await repository.SaveAsync(product);
            }
        }
    }
};


await MigrateReadModelDatabase(app);
await SeedProducts(app);
await app.RunAsync();