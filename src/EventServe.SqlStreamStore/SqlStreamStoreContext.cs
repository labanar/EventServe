using EventServe.SqlStreamStore.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace EventServe.SqlStreamStore
{
    public class SqlStreamStoreContext : DbContext
    {
        public DbSet<PeristentSubscriptionPosition> SubscriptionPositions { get; set; }
        public SqlStreamStoreContext(DbContextOptions<SqlStreamStoreContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PeristentSubscriptionPosition>(options =>
            {
                options.HasKey(x => x.SubscriptionId);
            });
        }
    }
}
