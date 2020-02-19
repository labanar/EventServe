using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Domain.Events
{
    public class ProductCreatedEvent : Event
    {
        public ProductCreatedEvent () { }
        public ProductCreatedEvent(Guid aggregateId, bool allowDefaultGuid = false) : base(aggregateId, allowDefaultGuid)
        {

        }

        public string Name { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }
        public string CurrencyCode { get; set; }
        public bool Available { get; set; }
    }
}
