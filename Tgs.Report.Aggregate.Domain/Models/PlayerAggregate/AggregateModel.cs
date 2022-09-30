using System;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record AggregateModel
{
    public Guid BrandId { get; set; }

    public Guid? AgentId { get; set; }

    public string PlayerCode { get; set; } = default!;

    public int Hour { get; set; }

    public DateTimeOffset ReportDate { get; set; }

    public int RegisteredCount { get; set; }

    public int FirstFundInCount { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal WithdrawalAmount { get; set; }

    public decimal CompanyAdjustAmount { get; set; }

    public decimal AgentAdjustAmount { get; set; }

    public decimal SystemAdjustAmount { get; set; }

    public decimal BonusAdjustAmount { get; set; }

    public decimal BonusAmount { get; set; }

    public decimal RebateAmount { get; set; }

    public decimal DividendAmount { get; set; }

    public decimal PromotionAmount { get; set; }

    public decimal BetAmount { get; set; }

    public decimal ValidBetAmount { get; set; }

    public decimal OriginalTurnover { get; set; }

    public decimal Turnover { get; set; }

    public decimal WinLossAmount { get; set; }

    public decimal AllAgentRebateAmount { get; set; }

    public decimal NoIncludeNetProfitCompanyAdjustAmount { get; set; }

    public decimal NoIncludeNetProfitAgentAdjustAmount { get; set; }

    public decimal NoIncludeNetProfitBonusAdjustAmount { get; set; }

    public decimal? AgentDividendPercentage { get; set; }

    public LTree? Path { get; set; }

    public LTree? ChiefAgentId { get; set; }

    public decimal UpgradeAmount { get; set; }

    public decimal SeasonAmount { get; set; }

    public decimal BirthdayAmount { get; set; }

    public int? VipLevel { get; set; }

    public Guid GameProviderId { get; set; }

    public int GameId { get; set; }

    public GameType GameType { get; set; }

    public decimal VipLiveDealerRebatePercentage { get; set; }

    public decimal VipSlotRebatePercentage { get; set; }

    public decimal VipCardGameRebatePercentage { get; set; }

    public decimal VipSportsRebatePercentage { get; set; }

    public decimal VipVirtualSportsRebatePercentage { get; set; }

    public decimal VipLotteryRebatePercentage { get; set; }
}