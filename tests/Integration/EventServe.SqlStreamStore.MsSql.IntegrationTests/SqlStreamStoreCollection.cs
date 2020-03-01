using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EventServe.SqlStreamStore.MsSql.IntegrationTests
{
    [CollectionDefinition("SqlStreamStore Collection")]
    public class EventStoreCollection : ICollectionFixture<EmbeddedMsSqlStreamStoreFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
