using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Domain.Events
{
    public class ProductAvailabilityChangedEvent : Event
    {
        public ProductAvailabilityChangedEvent() { }
        public ProductAvailabilityChangedEvent(Guid aggregateId) : base(aggregateId)
        {
        }

        public bool Available { get; set; }
    }
}
