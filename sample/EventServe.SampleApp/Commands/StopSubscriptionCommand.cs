using EventServe.Subscriptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Commands
{
    public class StopSubscriptionCommand : IRequest<Unit>
    {
        public Guid SubscriptionId { get; set; }
    }

    public class StopSubscriptionCommandHandler : IRequestHandler<StopSubscriptionCommand, Unit>
    {
        private readonly ISubscriptionRootManager _rootManager;

        public StopSubscriptionCommandHandler(ISubscriptionRootManager rootManager)
        {
            _rootManager = rootManager;
        }


        public async Task<Unit> Handle(StopSubscriptionCommand request, CancellationToken cancellationToken)
        {
            await _rootManager.StopSubscription(request.SubscriptionId);
            return Unit.Value;
        }
    }
}
