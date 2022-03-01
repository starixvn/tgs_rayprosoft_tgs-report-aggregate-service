using System;
using MediatR;

namespace Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate
{
    public record CreateGameProviderAggregateRequest
        (DateTimeOffset StartDate, Guid GameProviderId, int AuditLogId, string RedisKey) : IRequest<MessageResponse>;
}