using System;
using System.Threading.Tasks;

using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Domain.Interfaces
{
    public interface IRoundFactory
    {
        ValueTask<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end);

        ValueTask<RoundAggregateModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start, DateTimeOffset end);
    }
}
