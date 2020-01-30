using EventServe.Subscriptions.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.TestApp
{
    public class SubscriptionHandler :INotificationHandler<StreamSubscriptionEventNotification>
    {
        private readonly ILogger<SubscriptionHandler> _logger;

        public SubscriptionHandler(ILogger<SubscriptionHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(StreamSubscriptionEventNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(notification.Event.AggregateId.ToString());
            await notification.AcknowledgementCallback(notification.Event);
        }
    }
}
