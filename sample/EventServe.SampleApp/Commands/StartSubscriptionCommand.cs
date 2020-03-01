using EventServe.Subscriptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Commands
{
    public class StartSubscriptionCommand: IRequest<Unit>
    {
        public Guid SubscriptionId { get; set; }
    }

    public class StartSubscriptionCommandHandler : IRequestHandler<StartSubscriptionCommand, Unit>
    {
        private readonly ISubscriptionRootManager _rootManager;

        public StartSubscriptionCommandHandler(ISubscriptionRootManager rootManager)
        {
            _rootManager = rootManager;
        }


        public async Task<Unit> Handle(StartSubscriptionCommand request, CancellationToken cancellationToken)
        {
            await _rootManager.StartSubscription(request.SubscriptionId);
            return Unit.Value;
        }
    }
}
