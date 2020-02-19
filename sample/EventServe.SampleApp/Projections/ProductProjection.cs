using EventServe.Projections;
using EventServe.Projections.Partitioned;
using EventServe.SampleApp.Domain;
using EventServe.SampleApp.Domain.Events;
using System;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Projections
{
    public class ProductProjection : PartitionedProjection
    {
        public override Guid PartitionId => ProductId;
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }
        public string CurrencyCode { get; set; }
        public bool Available { get; set; }
    }


    public class ProductProjectionProfile: PartitionedProjectionProfile
    {
        public ProductProjectionProfile()
        {
            CreateProfile()
                .ProjectFromAggregateCategory<Product>()
                .OnTo<ProductProjection>()
                .HandleEvent<ProductCreatedEvent>()
                .HandleEvent<ProductPriceChangedEvent>()
                .HandleEvent<ProductAvailabilityChangedEvent>();
        }
    }


    public class ProductProjectionEventHandler : 
        IPartitionedProjectionEventHandler<ProductProjection, ProductCreatedEvent>,
        IPartitionedProjectionEventHandler<ProductProjection, ProductPriceChangedEvent>,
        IPartitionedProjectionEventHandler<ProductProjection, ProductAvailabilityChangedEvent>
    {
        public Task<ProductProjection> ProjectEvent(ProductProjection prevState, ProductCreatedEvent @event)
        {
            prevState.ProductId = @event.AggregateId;
            prevState.Name = @event.Name;
            prevState.Url = @event.Url;
            prevState.Price = @event.Price;
            prevState.CurrencyCode = @event.CurrencyCode;
            prevState.Available = @event.Available;
            return Task.FromResult(prevState);
        }

        public Task<ProductProjection> ProjectEvent(ProductProjection prevState, ProductPriceChangedEvent @event)
        {
            prevState.Price = @event.Price;
            prevState.CurrencyCode = @event.CurrencyCode;
            return Task.FromResult(prevState);
        }

        public Task<ProductProjection> ProjectEvent(ProductProjection prevState, ProductAvailabilityChangedEvent @event)
        {
            prevState.Available = @event.Available;
            return Task.FromResult(prevState);
        }
    }
}
