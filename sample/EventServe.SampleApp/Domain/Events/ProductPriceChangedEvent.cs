using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Domain.Events
{
    public class ProductPriceChangedEvent : Event
    {
        public ProductPriceChangedEvent() { }
        public ProductPriceChangedEvent(Guid aggregateId, bool allowDefaultGuid = false) : base(aggregateId, allowDefaultGuid)
        {

        }

        public double Price { get; set; }

        public string CurrencyCode { get; set; }
    }
}
