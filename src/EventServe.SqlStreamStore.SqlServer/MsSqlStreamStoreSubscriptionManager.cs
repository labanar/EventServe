using EventServe.SqlStreamStore.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.SqlServer
{
    public class MsSqlStreamStoreSubscriptionManager : ISqlStreamStoreSubscriptionManager
    {
        private readonly IServiceProvider _serviceProvider;

        public MsSqlStreamStoreSubscriptionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task CreateStreamSubscription(Guid subscriptionId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<MsSqlStreamStoreContext>())
                {
                    var position = new MsSqlStreamSubscriptionPosition()
                    {
                        Id = subscriptionId,
                        Name = subscriptionId.ToString()
                    };
                    await context.AddAsync(position);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<long?> GetStreamSubscriptionPosition(Guid subscriptionId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<MsSqlStreamStoreContext>())
                {
                    var position = await context.SubscriptionPositions.FindAsync(subscriptionId);
                    return position?.Position;
                }
            }
        }

        public async Task PersistAcknowledgement<T>(Guid subscriptionId, T @event) where T : Event
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<MsSqlStreamStoreContext>())
                {
                    var position = await context.SubscriptionPositions.FirstOrDefaultAsync(x => x.Id == subscriptionId);
                    position.Position = (position.Position == null) ? 0 : position.Position + 1;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
