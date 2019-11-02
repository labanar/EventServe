using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{
    [CollectionDefinition("EventStore Collection")]
    public class EventStoreCollection : ICollectionFixture<EmbeddedEventStoreFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
