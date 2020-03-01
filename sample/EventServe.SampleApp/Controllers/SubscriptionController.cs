using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventServe.SampleApp.Commands;
using EventServe.SampleApp.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventServe.SampleApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetSubscriptions()
        {
            var subscriptions = await _mediator.Send(new GetSubscriptionsQuery());
            return new JsonResult(subscriptions);
        }


        [HttpGet("{id}/reset")]
        public async Task<IActionResult> ResetSubscription(Guid id)
        {
            await _mediator.Send(new ResetPersistentSubscriptionCommand() { SubscriptionId = id });
            return new OkResult();
        }

        [HttpGet("{id}/start")]
        public async Task<IActionResult> StartSubscriptions(Guid id)
        {
            await _mediator.Send(new StartSubscriptionCommand() { SubscriptionId = id });
            return new OkResult();
        }

        [HttpGet("{id}/stop")]
        public async Task<IActionResult> StopSubscription(Guid id)
        {
            await _mediator.Send(new StopSubscriptionCommand() { SubscriptionId = id });
            return new OkResult();
        }

    }
}