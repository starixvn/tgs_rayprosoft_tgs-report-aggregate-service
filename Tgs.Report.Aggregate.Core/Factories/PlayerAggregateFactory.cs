using Tgs.Report.Aggregate.Core.Extensions;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Factories;

public class PlayerAggregateFactory : IPlayerAggregateFactory
{
	private readonly ITgsRepository _repository;

	public PlayerAggregateFactory(ITgsRepository repository)
	{
		_repository = repository;
	}

	public Task CreatePlayerAggregatesAsync(PlayerAggregateModel[] models)
	{
		_ = _repository.PlayerAggregates.AddRangeAsync(models.Select(model => new PlayerAggregate
		{
			BrandId = model.BrandId,
			AgentId = model.AgentId,
			PlayerCode = model.PlayerCode,
			Hour = model.Hour,
			ReportDate = model.ReportDate,
			CreatedOn = model.CreatedOn,
			RegisteredCount = model.RegisteredCount,
			FirstFundInCount = model.FirstFundInCount,
			DepositAmount = model.DepositAmount.Truncate(2),
			WithdrawalAmount = model.WithdrawalAmount.Truncate(2),
			CompanyAdjustAmount = model.CompanyAdjustAmount.Truncate(2),
			AgentAdjustAmount = model.AgentAdjustAmount.Truncate(2),
			SystemAdjustAmount = model.SystemAdjustAmount.Truncate(2),
			BonusAmount = model.BonusAmount.Truncate(2),
			RebateAmount = model.RebateAmount.Truncate(2),
			PromotionAmount = model.PromotionAmount.Truncate(2),
			DividendAmount = model.DividendAmount.Truncate(2),
			BetAmount = model.BetAmount.Truncate(2),
			ValidBetAmount = model.ValidBetAmount.Truncate(2),
			OriginalTurnover = model.OriginalTurnover.Truncate(2),
			Turnover = model.Turnover.Truncate(2),
			WinLossAmount = model.WinLossAmount.Truncate(2),
			AllAgentRebateAmount = model.AllAgentRebateAmount.Truncate(2),
			NoIncludeNetProfitCompanyAdjustAmount = model.NoIncludeNetProfitCompanyAdjustAmount.Truncate(2),
			NoIncludeNetProfitAgentAdjustAmount = model.NoIncludeNetProfitAgentAdjustAmount.Truncate(2)
		}));

		return _repository.SaveChangesAsync();
	}
}
