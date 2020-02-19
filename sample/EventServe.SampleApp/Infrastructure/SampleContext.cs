using EventServe.SampleApp.Projections;
using Microsoft.EntityFrameworkCore;

namespace EventServe.SampleApp.Infrastructure
{
    public class SampleContext: DbContext
    {
        public SampleContext(DbContextOptions<SampleContext> contextOptions): base(contextOptions)
        {

        }

        public DbSet<ProductProjection> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("Sample");
            builder.Entity<ProductProjection>(options =>
            {
                options.HasKey(x => x.ProductId);
            });
        }
    }
}
