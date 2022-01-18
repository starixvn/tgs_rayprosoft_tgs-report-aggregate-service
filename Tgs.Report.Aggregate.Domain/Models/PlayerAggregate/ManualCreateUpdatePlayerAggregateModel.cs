using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate
{
    public record ManualCreateUpdatePlayerAggregateModel(string Date, int Hour) : IRequest<MessageResponse>;
}