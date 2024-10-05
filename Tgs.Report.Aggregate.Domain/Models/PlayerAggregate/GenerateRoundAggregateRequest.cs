using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record GenerateRoundAggregateRequest : IRequest<MessageResponse>;
