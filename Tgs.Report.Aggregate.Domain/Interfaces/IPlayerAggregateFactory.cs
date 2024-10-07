using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IPlayerAggregateFactory
{
    Task CreatePlayerAggregatesAsync(PlayerAggregateModel[] request);

    Task RemovePlayerAggregatesAsync(DateTimeOffset start, DateTimeOffset end);

    ValueTask<(
            PlayerAggregateModel[] playerAggregate,
            PlayerGameTypeAggregateModel[] playerAggregateByGameType,
            PlayerBonusAggregateModel[] playerBonusAggregate)>
        GetPlayerAggregatesAsync(
            DateTimeOffset start,
            DateTimeOffset end);
}