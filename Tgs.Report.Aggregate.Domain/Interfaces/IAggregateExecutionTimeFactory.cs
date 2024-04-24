using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IAggregateExecutionTimeFactory
{
    ValueTask<int> CreateExecutionTimeAsync(AggregateType type,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    ValueTask<bool> ExecutionTimeIsExistsAsync(AggregateType type,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);
}