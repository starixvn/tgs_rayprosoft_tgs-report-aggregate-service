using System.ComponentModel;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoundAggregateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoundAggregateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Description("Create round aggregate")]
        public Task<MessageResponse> CreateRoundAggregateAsync() =>
            _mediator.Send(new CreateRoundAggregateRequest());

        [HttpPost("fillPlayerBucketMinutely")]
        [Description("Fill player bucket minutely")]
        public Task<MessageResponse> FillPlayerBucketMinutelyAsync() =>
            _mediator.Send(new FillPlayerBucketMinutelyRequest());

        [HttpPost("manual")]
        [Description("Manual create or update round aggregate")]
        public Task<MessageResponse> ManualCreateUpdateRoundAggregateAsync(
            ManualCreateRoundAggregateModel request) =>
            _mediator.Send(request);

        [HttpPost("manualFillPlayerBucketMinutely")]
        [Description("Manual fill player bucket minutely")]
        public Task<MessageResponse>
            ManualFillPlayerBucketMinutelyAsync(ManualFillPlayerBucketMinutelyRequest request) =>
            _mediator.Send(request);
    }
}
