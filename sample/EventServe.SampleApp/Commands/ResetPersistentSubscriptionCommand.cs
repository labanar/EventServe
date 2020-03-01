using EventServe.Subscriptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.SampleApp.Commands
{
    public class ResetPersistentSubscriptionCommand: IRequest<Unit>
    {
        public Guid SubscriptionId { get; set; }
    }

    public class ResetPersistentSubscriptionCommandHandler : IRequestHandler<ResetPersistentSubscriptionCommand, Unit>
    {
        private readonly ISubscriptionManager _manager;

        public ResetPersistentSubscriptionCommandHandler(ISubscriptionManager manager)
        {
            _manager = manager;
        }

        public async Task<Unit> Handle(ResetPersistentSubscriptionCommand request, CancellationToken cancellationToken)
        {
            await _manager.Reset(request.SubscriptionId);
            return Unit.Value;
        }
    }
}
