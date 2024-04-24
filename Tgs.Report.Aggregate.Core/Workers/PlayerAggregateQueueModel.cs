namespace Tgs.Report.Aggregate.Core.Workers;

public record PlayerAggregateQueueModel(
    DateTimeOffset ReportDate,
    DateTimeOffset Start,
    DateTimeOffset End);