using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Factories;

public class AggregateExecutionTimeFactory(ITgsRepository repository) : IAggregateExecutionTimeFactory
{
    public async ValueTask<int> CreateExecutionTimeAsync(AggregateType type,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        _ = await repository.AggregateExecutionTimes.AddAsync(new AggregateExecutionTime
            {
                AggregateType = (short)type,
                From = from.UtcDateTime,
                To = to.UtcDateTime
            },
            cancellationToken);

        return await repository.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<bool> ExecutionTimeIsExistsAsync(AggregateType type,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await repository.AggregateExecutionTimes.AnyAsync(
            x => x.AggregateType == (short)type && x.From == from.ToUniversalTime() && x.To == to.ToUniversalTime(),
            cancellationToken);
    }
}