using System;
using System.Threading.Tasks;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IPlayerAggregateFactory
{
    Task CreatePlayerAggregatesAsync(PlayerAggregateModel[] request);

    ValueTask<(
            PlayerAggregateModel[] playerAggregate, 
            PlayerGameTypeAggregateModel[] playerAggregateByGameType,
            PlayerBonusAggregateModel[] playerBonusAggregate)>
        GetPlayerAggregatesAsync(
            DateTimeOffset start,
            DateTimeOffset end);
}