using EventServe.Projections;
using EventServe.Projections.Partitioned;
using EventServe.SampleApp.Domain;
using EventServe.SampleApp.Domain.Events;
using System;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Projections
{
    public class AnotherProjection : PartitionedProjection
    {
        public override Guid PartitionId => ProductId;
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }
        public string CurrencyCode { get; set; }
        public bool Available { get; set; }
        public override Guid LastEventId { get; set; }
    }

    public class AnotherProjectionProfile : PartitionedProjectionProfile<AnotherProjection>
    {
        public AnotherProjectionProfile()
        {
            CreateProfile<AnotherProjection>()
                .ProjectFromAggregateCategory<Product>()
                .HandleEvent<ProductCreatedEvent>()
                .HandleEvent<ProductPriceChangedEvent>()
                .HandleEvent<ProductAvailabilityChangedEvent>();
        }
    }

    public class AnotherProjectionEventHandler :
        IPartitionedProjectionEventHandler<AnotherProjection, ProductCreatedEvent>,
        IPartitionedProjectionEventHandler<AnotherProjection, ProductPriceChangedEvent>,
        IPartitionedProjectionEventHandler<AnotherProjection, ProductAvailabilityChangedEvent>
    {
        public Task<AnotherProjection> ProjectEvent(AnotherProjection prevState, ProductCreatedEvent @event)
        {
            prevState.ProductId = @event.AggregateId;
            prevState.Name = @event.Name;
            prevState.Url = @event.Url;
            prevState.Price = @event.Price;
            prevState.CurrencyCode = @event.CurrencyCode;
            prevState.Available = @event.Available;
            return Task.FromResult(prevState);
        }

        public Task<AnotherProjection> ProjectEvent(AnotherProjection prevState, ProductPriceChangedEvent @event)
        {
            prevState.Price = @event.Price;
            prevState.CurrencyCode = @event.CurrencyCode;
            return Task.FromResult(prevState);
        }

        public Task<AnotherProjection> ProjectEvent(AnotherProjection prevState, ProductAvailabilityChangedEvent @event)
        {
            prevState.Available = @event.Available;
            return Task.FromResult(prevState);
        }
    }
}
