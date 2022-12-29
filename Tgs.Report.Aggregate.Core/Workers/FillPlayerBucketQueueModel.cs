namespace Tgs.Report.Aggregate.Core.Workers;

    public record FillPlayerBucketQueueModel(
        Guid RoundId,
        string PlayerCode,
        decimal Turnover,
        DateTimeOffset StartTime);