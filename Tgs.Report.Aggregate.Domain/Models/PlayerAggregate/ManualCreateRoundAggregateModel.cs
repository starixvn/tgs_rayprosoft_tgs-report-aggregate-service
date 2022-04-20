using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record ManualCreateRoundAggregateModel(string Date, int Hour) : IRequest<MessageResponse>;
