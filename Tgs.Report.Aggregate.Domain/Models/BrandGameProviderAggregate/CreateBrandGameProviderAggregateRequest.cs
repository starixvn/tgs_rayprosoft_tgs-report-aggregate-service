using System;
using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate
{
    public record CreateBrandGameProviderAggregateRequest
        (DateTimeOffset StartDate, Guid BrandId, int AuditLogId, string RedisKey) : IRequest<MessageResponse>;
}