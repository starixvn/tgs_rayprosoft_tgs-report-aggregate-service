using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandGameProviderAggregateController : ControllerBase
{
	private readonly IMediator _mediator;

	public BrandGameProviderAggregateController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpDelete]
	[Description("Remove brand game provider aggregate")]
	public Task<MessageResponse> RemoveBrandGameProviderAggregateAsync(
		[FromQuery] RemoveBrandGameProviderAggregateRequest request) =>
		_mediator.Send(request);
}
