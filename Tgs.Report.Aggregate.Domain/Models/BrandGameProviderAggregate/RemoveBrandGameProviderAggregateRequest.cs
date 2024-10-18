using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

public record RemoveBrandGameProviderAggregateRequest(Guid? BrandId, DateTimeOffset StartDate, DateTimeOffset EndDate)
    : IRequest<MessageResponse>;