﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe.SqlStreamStore.MsSql
{
    public class MsSqlStreamStoreOptions
    {
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
    }
}
