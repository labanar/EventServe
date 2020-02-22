using System.Threading.Tasks;

namespace EventServe.Subscriptions.Persistent
{
    public interface IPersistentSubscriptionResetHandler<TProfile>
        where TProfile: PersistentSubscriptionProfile
    {
        Task HandleReset();
    }
}
