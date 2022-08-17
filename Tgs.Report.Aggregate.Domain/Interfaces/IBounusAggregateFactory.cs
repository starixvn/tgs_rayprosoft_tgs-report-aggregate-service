using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IPlayerBonusAggregateFactory
{
    ValueTask<int>
        CreatePlayerBonusAggregatesAsync(
            PlayerBonusAggregateModel[] aggregate,
            CancellationToken cancellationToken);

}
