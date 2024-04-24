using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IRoundAggregateFactory
{
    ValueTask<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end);
}