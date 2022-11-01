using Tgs.Report.Aggregate.Core.Extensions;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerGameAggregateFactory : IPlayerGameAggregateFactory
{
	private readonly ITgsRepository _repository;

	public PlayerGameAggregateFactory(ITgsRepository repository)
	{
		_repository = repository;
	}

	public Task CreatePlayerGameAggregatesAsync(
		PlayerGameTypeAggregateModel[] models)
	{
		_ = _repository.PlayerGameRebateAggregates.AddRangeAsync(models.Select(model => new PlayerGameRebateAggregate
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
			CreatedOn = model.CreatedOn.UtcDateTime,
			BetAmount = model.BetAmount.Truncate(2),
			ValidBetAmount = model.ValidBetAmount.Truncate(2),
			OriginalTurnover = model.OriginalTurnover.Truncate(2),
			Turnover = model.Turnover.Truncate(2),
			WinLoss = model.WinLoss.Truncate(2),
			VipRebateAmount = model.VipRebateAmount.Truncate(2),
			VipLiveDealerRebatePercentage = model.VipLiveDealerRebatePercentage,
			VipSlotRebatePercentage = model.VipSlotRebatePercentage,
			VipCardGameRebatePercentage = model.VipCardGameRebatePercentage,
			VipSportsRebatePercentage = model.VipSportsRebatePercentage,
			VipVirtualSportsRebatePercentage = model.VipVirtualSportsRebatePercentage,
			VipLotteryRebatePercentage = model.VipLotteryRebatePercentage
		}));

		return _repository.SaveChangesAsync();
	}
}
