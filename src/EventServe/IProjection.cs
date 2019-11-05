using System;
using System.Collections.Generic;
using System.Text;

namespace EventServe {
    public interface IProjection {
        int Version { get; }
        void IncrementVersion();
    }
}