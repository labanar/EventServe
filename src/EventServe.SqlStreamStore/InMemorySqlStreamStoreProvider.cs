using SqlStreamStore;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore
{
    public interface ISqlStreamStoreProvider
    {
        Task<IStreamStore> GetStreamStore();
    }

    //TODO - abstract factory this maybe?
    public interface ISqlStreamStoreSubscriptionStoreProvider: ISqlStreamStoreProvider
    {

    }

    public class InMemorySqlStreamStoreProvider: ISqlStreamStoreProvider, ISqlStreamStoreSubscriptionStoreProvider
    {
        private readonly InMemoryStreamStore _store;

        public InMemorySqlStreamStoreProvider()
        {
            _store = new InMemoryStreamStore();
        }

        public Task<IStreamStore> GetStreamStore()
        {
            return Task.FromResult<IStreamStore>(_store);
        }
    }


}
