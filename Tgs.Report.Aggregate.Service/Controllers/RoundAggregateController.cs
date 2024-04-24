using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoundAggregateController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [Description("Create round aggregate")]
    public Task<MessageResponse> CreateRoundAggregateAsync()
    {
        return mediator.Send(new CreateRoundAggregateRequest());
    }

    [HttpPost("manual")]
    [Description("Manual create or update round aggregate")]
    public Task<MessageResponse> ManualCreateUpdateRoundAggregateAsync(
        ManualCreateRoundAggregateModel request)
    {
        return mediator.Send(request);
    }
}