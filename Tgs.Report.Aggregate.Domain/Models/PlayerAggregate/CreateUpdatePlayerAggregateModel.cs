using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate
{
    public record CreateUpdatePlayerAggregateModel : IRequest<MessageResponse>;
}