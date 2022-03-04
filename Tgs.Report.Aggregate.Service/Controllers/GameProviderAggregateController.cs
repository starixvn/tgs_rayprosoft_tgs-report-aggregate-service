using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameProviderAggregateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GameProviderAggregateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpDelete]
        [Description("Remove game provider aggregate")]
        public Task<MessageResponse> RemoveGameProviderAggregateAsync(
            [FromQuery] RemoveGameProviderAggregateRequest request) =>
            _mediator.Send(request);
    }
}