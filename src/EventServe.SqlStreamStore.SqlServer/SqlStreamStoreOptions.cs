using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.SqlServer
{
    public class SqlStreamStoreOptions
    {
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
    }
}
