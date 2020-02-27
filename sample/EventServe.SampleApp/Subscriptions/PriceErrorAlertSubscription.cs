using System;
using System.Threading.Tasks;
using EventServe.SampleApp.Domain;
using EventServe.SampleApp.Domain.Events;
using EventServe.SampleApp.Infrastructure;
using EventServe.Subscriptions;
using EventServe.Subscriptions.Persistent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventServe.SampleApp.Subscriptions
{
    /// <summary>
    /// Some retailers honour orders containing items with significant
    /// price errors.This subscription scans for price discrepancies,
    /// writing to the log when the price change is >40%.
    /// </summary>
    public class PriceErrorAlertSubscription : PersistentSubscriptionProfile
    {
        public PriceErrorAlertSubscription()
        {
            CreateProfile()
                .SubscribeToAggregateCategory<Product>()
                .HandleEvent<ProductCreatedEvent>()
                .HandleEvent<ProductPriceChangedEvent>();
        }
    }

    public class PriceErrorAlertSubscriptionHandler:
            ISubscriptionEventHandler<PriceErrorAlertSubscription, ProductCreatedEvent>,
            ISubscriptionEventHandler<PriceErrorAlertSubscription, ProductPriceChangedEvent>,
            IPersistentSubscriptionResetHandler<PriceErrorAlertSubscription>
    {
        private readonly SampleContext _context;
        private readonly ILogger<PriceErrorAlertSubscriptionHandler> _logger;

        public PriceErrorAlertSubscriptionHandler(
            SampleContext context,
            ILogger<PriceErrorAlertSubscriptionHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task HandleEvent(ProductCreatedEvent @event)
        {
            if (string.IsNullOrEmpty(@event.CurrencyCode))
                return;

            var lastPrice = new PriceErrorAlertLastPrice
            {
                ProductId = @event.AggregateId,
                Price = @event.Price,
                CurrencyCode = @event.CurrencyCode,
                DateLastModified = @event.EventDate
            };
            await _context.AddAsync(lastPrice);
            await _context.SaveChangesAsync();
        }

        public async Task HandleEvent(ProductPriceChangedEvent @event)
        {
            //Check the last price
            var lastPrice = await _context.PriceErrorAlertLastPrices.FindAsync(@event.AggregateId);

            //Compare
            //We should be checking the currency code and exchanging to a default currency before comparing prices
            if (@event.Price > lastPrice.Price)
            {
                lastPrice.Price = @event.Price;
                lastPrice.CurrencyCode = @event.CurrencyCode;
                lastPrice.DateLastModified = @event.EventDate;
                await _context.SaveChangesAsync();
                await _context.DisposeAsync();
                return;
            }

            //Notify
            var discount = 1 - (@event.Price / lastPrice.Price);
            if (discount > 0.4)
                _logger.LogInformation($"[{@event.AggregateId}] Potential price error detected: [{(discount * 100.0).ToString("F2")}%] {lastPrice.Price.ToString("C")} {lastPrice.CurrencyCode} -> {@event.Price.ToString("C")} {@event.CurrencyCode}");


            lastPrice.Price = @event.Price;
            lastPrice.CurrencyCode = @event.CurrencyCode;
            lastPrice.DateLastModified = @event.EventDate;
            await _context.SaveChangesAsync();
            await _context.DisposeAsync();
        }

        [Obsolete]
        public async Task HandleReset()
        {
            var cmd = $"TRUNCATE TABLE [Sample].[{nameof(_context.PriceErrorAlertLastPrices)}];";
            await _context.Database.ExecuteSqlCommandAsync(cmd);
        }
    }


    public class PriceErrorAlertLastPrice
    {
        public Guid ProductId { get; set; }
        public double Price { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime DateLastModified { get; set; }
    }
}
