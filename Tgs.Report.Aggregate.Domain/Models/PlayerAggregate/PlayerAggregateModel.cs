using System;

namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate
{
    public record PlayerAggregateModel
    {
        public Guid BrandId { get; set; }

        public Guid? AgentId { get; set; }

        public string PlayerCode { get; set; } = default!;

        public int Hour { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset ReportDate { get; set; }

        public int RegisteredCount { get; set; }

        public int FirstFundInCount { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal WithdrawalAmount { get; set; }

        public decimal AdjustmentAmount { get; set; }

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
    }
}