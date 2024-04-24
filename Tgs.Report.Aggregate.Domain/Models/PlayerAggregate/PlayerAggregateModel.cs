using System;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record PlayerAggregateModel
{
    public Guid BrandId { get; set; }

    public Guid? AgentId { get; set; }

    public string PlayerCode { get; set; } = default!;

    public int Hour { get; set; }

    public DateTimeOffset ReportDate { get; set; }

    public DateTimeOffset ReportDateTime { get; set; }

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

    public decimal? ChiefAgentDividendPercentage { get; set; }

    public string? AgentDividendPercentageTreeData { get; set; }

    public int RoundCount { get; set; }

    public decimal UpgradeAmount { get; set; }

    public decimal SeasonAmount { get; set; }

    public decimal BirthdayAmount { get; set; }

    public decimal PlatformChargePercentage { get; set; }
}