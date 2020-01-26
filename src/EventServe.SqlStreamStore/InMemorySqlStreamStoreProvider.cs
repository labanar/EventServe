using SqlStreamStore;
using System.Threading.Tasks;

namespace EventServe.SqlStreamStore
{
    public interface ISqlStreamStoreProvider
    {
        Task<IStreamStore> GetStreamStore();
    }

    public class InMemorySqlStreamStoreProvider: ISqlStreamStoreProvider
    {
        private readonly InMemoryStreamStore _store;

        public InMemorySqlStreamStoreProvider()
        {
            _store = new InMemoryStreamStore();
        }

        public async Task<IStreamStore> GetStreamStore()
        {
            return _store;
        }
    }
}
