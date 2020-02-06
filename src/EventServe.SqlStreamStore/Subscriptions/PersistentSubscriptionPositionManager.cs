using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore.Subscriptions
{
    public interface IPersistentSubscriptionPositionManager
    {
        Task<long?> GetSubscriptionPosition(string subscriptionName);
        Task IncrementSubscriptionPosition(string subscriptionName);
    }


    public class PersistentSubscriptionPositionManager : IPersistentSubscriptionPositionManager
    {
        private readonly IServiceProvider _serviceProvider;

        public PersistentSubscriptionPositionManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<long?> GetSubscriptionPosition(string subscriptionName)
        {
            using(var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SqlStreamStoreContext>())
            {
                var subscriptionPosition = await context.SubscriptionPositions.FirstOrDefaultAsync(x => x.Name == subscriptionName);
                return subscriptionPosition?.Position;
            }

        }

        public async Task IncrementSubscriptionPosition(string subscriptionName)
        {
            using (var context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SqlStreamStoreContext>())
            {
                var subscriptionPosition = await context.SubscriptionPositions.FirstOrDefaultAsync(x => x.Name == subscriptionName);
                if (subscriptionPosition == null)
                {
                    subscriptionPosition = new PeristentSubscriptionPosition
                    {
                        Name = subscriptionName,
                        Position = 0
                    };
                    await context.AddAsync(subscriptionPosition);
                    await context.SaveChangesAsync();
                    return;
                }

                subscriptionPosition.Position += 1;
                await context.SaveChangesAsync();
            }
        }
    }
}
