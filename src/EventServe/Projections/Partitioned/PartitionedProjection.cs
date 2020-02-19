using System;

namespace EventServe.Projections
{
    public abstract class PartitionedProjection
    {
        public abstract Guid PartitionId { get; }
    }
}
