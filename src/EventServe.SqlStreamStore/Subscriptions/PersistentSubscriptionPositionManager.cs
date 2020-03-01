using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public interface IPersistentSubscriptionPositionManager
    {
        Task<long?> GetSubscriptionPosition(Guid subscriptionId, bool createIfNotExists);
        Task SetSubscriptionPosition(Guid subscriptionId, long? position);
        Task ResetSubscriptionPosition(Guid subscriptionId);
    }


    public class PersistentSubscriptionPositionManager : IPersistentSubscriptionPositionManager
    {
        private readonly IServiceProvider _serviceProvider;

        public PersistentSubscriptionPositionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<long?> GetSubscriptionPosition(Guid subscriptionId, bool createIfNotExists)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<SqlStreamStoreContext>();
                {
                    var subscriptionPosition = await context.SubscriptionPositions.FindAsync(subscriptionId);

                    if (createIfNotExists && subscriptionPosition == default)
                    {
                        subscriptionPosition = new PeristentSubscriptionPosition
                        {
                            SubscriptionId = subscriptionId
                        };
                        await context.SubscriptionPositions.AddAsync(subscriptionPosition);
                        await context.SaveChangesAsync();
                    }

                    //context.Entry(subscriptionPosition).State = EntityState.Detached;
                    return subscriptionPosition?.Position;
                }
            }

        }

        public async Task SetSubscriptionPosition(Guid subscriptionId, long? position)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<SqlStreamStoreContext>())
                {
                    var subscriptionPosition = new PeristentSubscriptionPosition
                    {
                        SubscriptionId = subscriptionId,
                        Position = position
                    };
                    context.Attach(subscriptionPosition);
                    context.Entry(subscriptionPosition).Property(p => p.Position).IsModified = true;
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task ResetSubscriptionPosition(Guid subscriptionId)
        {
            await SetSubscriptionPosition(subscriptionId, null);
        }
    }
}
