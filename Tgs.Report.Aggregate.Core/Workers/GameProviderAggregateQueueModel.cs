namespace Tgs.Report.Aggregate.Core.Workers;

public record GameProviderAggregateQueueModel(
    DateTimeOffset StartDate,
    Guid GameProviderId,
    int AuditLogId,
    string RedisKey);