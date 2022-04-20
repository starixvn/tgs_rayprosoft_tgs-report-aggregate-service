using System.ComponentModel;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerAggregateController : ControllerBase
{
	private readonly IMediator _mediator;

	public PlayerAggregateController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpPost]
	[Description("Create player aggregate")]
	public Task<MessageResponse> CreatePlayerAggregateAsync() =>
		_mediator.Send(new CreateUpdatePlayerAggregateModel());

	[HttpPost("manual")]
	[Description("Manual create or update player aggregate")]
	public Task<MessageResponse> ManualCreateUpdatePlayerAggregateAsync(
		ManualCreateUpdatePlayerAggregateModel request) =>
		_mediator.Send(request);
}
