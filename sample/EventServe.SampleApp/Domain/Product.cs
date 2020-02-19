using EventServe.SampleApp.Domain.Commands;
using EventServe.SampleApp.Domain.Events;
using System;

namespace EventServe.SampleApp.Domain
{
    public class Product : AggregateRoot
    {
        public override Guid Id => _id;

        private Guid _id;

        private string _url;
        private string _name;
        private bool _available;
        private double _price;
        private string _currencyCode;
        private Product() { }


        public Product(ResetProductCommand command)
        {
            ApplyChange(new ProductCreatedEvent(command.ProductId)
            {
                Name = command.Name,
                Url = command.Url,
                Price = command.Price,
                CurrencyCode = command.CurrencyCode,
                Available = command.Available
            });
        }

        public void ResetProduct(ResetProductCommand command)
        {
            if (command.Price != _price || command.CurrencyCode != _currencyCode)
                ApplyChange(new ProductPriceChangedEvent(_id)
                {
                    Price = command.Price,
                    CurrencyCode = command.CurrencyCode
                });

            if (command.Available != _available)
                ApplyChange(new ProductAvailabilityChangedEvent(_id)
                {
                    Available = command.Available,
                });
        }

        private void Apply(ProductCreatedEvent @event)
        {
            _id = @event.AggregateId;
            _name = @event.Name;
            _url = @event.Url;
            _price = @event.Price;
            _currencyCode = @event.CurrencyCode;
            _available = @event.Available;
        }

        private void Apply(ProductAvailabilityChangedEvent @event)
        {
            _available = @event.Available;
        }

        private void Apply(ProductPriceChangedEvent @event)
        {
            _price = @event.Price;
            _currencyCode = @event.CurrencyCode;
        }
    }
}
