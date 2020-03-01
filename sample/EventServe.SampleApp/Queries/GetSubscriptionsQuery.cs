using EventServe.Subscriptions;
using EventServe.Subscriptions.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Queries
{
    public class GetSubscriptionsQueryResponse
    {
        public List<SubscriptionReadModel> Subscriptions { get; set; } = new List<SubscriptionReadModel>();
        public class SubscriptionReadModel
        {
            public Guid Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public long? Position { get; set; }
            public string Status { get; set; }
            public DateTime? StartDate { get; set; }
        }
    }

    public class GetSubscriptionsQuery: IRequest<GetSubscriptionsQueryResponse>
    {

    }

    public class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, GetSubscriptionsQueryResponse>
    {
        private readonly ISubscriptionManager _manager;

        public GetSubscriptionsQueryHandler(ISubscriptionManager manager)
        {
            _manager = manager;
        }

        public async Task<GetSubscriptionsQueryResponse> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            var response = new GetSubscriptionsQueryResponse();
            await foreach (var subscription in _manager.GetSubscriptions())
                response.Subscriptions.Add(new GetSubscriptionsQueryResponse.SubscriptionReadModel
                {
                    Id = subscription.id,
                    Type = subscription.type,
                    Name = subscription.name,
                    Position = subscription.position,
                    Status = GetStatusString(subscription.status),
                    StartDate = subscription.startDate
                });

            return response;
        }

        private Func<SubscriptionConnectionStatus, string> GetStatusString = status =>
           (status == SubscriptionConnectionStatus.Connected)
               ? "Connected"
               : (status == SubscriptionConnectionStatus.Disconnected)
                    ? "Disconnected" 
                    : "Idle";
    }
}
