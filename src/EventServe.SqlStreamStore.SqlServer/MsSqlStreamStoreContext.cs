using EventServe.SqlStreamStore.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace EventServe.SqlStreamStore.SqlServer
{
    public class MsSqlStreamStoreContext : DbContext
    {
        public DbSet<MsSqlStreamSubscriptionPosition> SubscriptionPositions { get; set; }

        protected MsSqlStreamStoreContext()
        {
        }

        public MsSqlStreamStoreContext(DbContextOptions<MsSqlStreamStoreContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MsSqlStreamSubscriptionPosition>()
                .HasIndex(x => x.Name).IsUnique();

        }
    }
}
