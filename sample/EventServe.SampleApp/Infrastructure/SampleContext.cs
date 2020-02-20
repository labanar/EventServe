using EventServe.SampleApp.Projections;
using EventServe.SampleApp.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace EventServe.SampleApp.Infrastructure
{
    public class SampleContext: DbContext
    {
        public SampleContext(DbContextOptions<SampleContext> contextOptions): base(contextOptions)
        {

        }

        public DbSet<ProductProjection> Products { get; set; }
        public DbSet<PriceErrorAlertLastPrice> PriceErrorAlertLastPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("Sample");
            builder.Entity<ProductProjection>(options =>
            {
                options.HasKey(x => x.ProductId);
            });

            builder.Entity<PriceErrorAlertLastPrice>(options =>
            {
                options.HasKey(x => x.ProductId);
            });
        }
    }
}
