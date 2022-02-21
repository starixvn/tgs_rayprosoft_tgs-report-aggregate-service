using System;
using System.Threading.Tasks;

using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.Domain.Interfaces
{
    public interface IRoundAggregateFactory
    {
        ValueTask<(PlayerAggregateModel[] playerAggregates, PlayerGameTypeAggregateModel[] playerAggregateByGameType)>
            GetPlayerAggregatesAsync(
                DateTimeOffset start,
                DateTimeOffset end);
    }
}
