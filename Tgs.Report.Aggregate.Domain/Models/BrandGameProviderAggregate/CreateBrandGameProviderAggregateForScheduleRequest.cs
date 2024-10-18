using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

public record CreateBrandGameProviderAggregateForScheduleRequest(
    Guid BrandId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate) : IRequest<MessageResponse>;