using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IRoundFactory
{
    ValueTask<RoundMinutelyModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end);
}