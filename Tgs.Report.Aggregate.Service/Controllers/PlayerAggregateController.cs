using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerAggregateController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [Description("Create player aggregate")]
    public Task<MessageResponse> CreatePlayerAggregateAsync()
    {
        return mediator.Send(new CreateUpdatePlayerAggregateModel());
    }

    [HttpPost("generate-player-aggregate")]
    [Description("Generate player aggregate")]
    public Task<MessageResponse> GeneratePlayerAggregateAsync()
    {
        return mediator.Send(new GeneratePlayerAggregateRequest());
    }

    [HttpPost("manual")]
    [Description("Manual create or update player aggregate")]
    public Task<MessageResponse> ManualCreateUpdatePlayerAggregateAsync(
        ManualCreateUpdatePlayerAggregateModel request)
    {
        return mediator.Send(request);
    }
}