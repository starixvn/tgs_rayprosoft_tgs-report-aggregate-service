namespace Tgs.Report.Aggregate.Core.Workers;

public record BrandGameProviderAggregateQueueModel(
    DateTimeOffset StartDate,
    Guid BrandId,
    int AuditLogId,
    string RedisKey);