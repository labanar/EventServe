using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Domain.Commands
{
    public class ResetProductCommand
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }
        public string CurrencyCode { get; set; }
        public bool Available { get; set; }
    }
}
