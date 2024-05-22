using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerGameAggregateFactory(ITgsRepository repository) : IPlayerGameAggregateFactory
{
    public Task CreatePlayerGameAggregatesAsync(
        PlayerGameTypeAggregateModel[] models)
    {
        _ = repository.PlayerGameRebateAggregates.AddRangeAsync(models.Select(model => new PlayerGameRebateAggregate
        {
            BrandId = model.BrandId,
            AgentId = model.AgentId,
            GameProviderId = model.GameProviderId,
            PlayerCode = model.PlayerCode,
            VipLevel = model.VipLevel,
            GameId = model.GameId,
            GameType = (short)model.GameType,
            Hour = model.Hour,
            ReportDate = model.ReportDate.UtcDateTime,
            ReportDateTime = model.ReportDateTime.UtcDateTime,
            CreatedOn = model.CreatedOn.UtcDateTime,
            BetAmount = model.BetAmount,
            ValidBetAmount = model.ValidBetAmount,
            OriginalTurnover = model.OriginalTurnover,
            Turnover = model.Turnover,
            WinLoss = model.WinLoss,
            VipRebateAmount = model.VipRebateAmount,
            VipLiveDealerRebatePercentage = model.VipLiveDealerRebatePercentage,
            VipSlotRebatePercentage = model.VipSlotRebatePercentage,
            VipCardGameRebatePercentage = model.VipCardGameRebatePercentage,
            VipSportsRebatePercentage = model.VipSportsRebatePercentage,
            VipVirtualSportsRebatePercentage = model.VipVirtualSportsRebatePercentage,
            VipLotteryRebatePercentage = model.VipLotteryRebatePercentage,
            RoundCount = model.RoundCount
        }));

        return repository.SaveChangesAsync();
    }
}