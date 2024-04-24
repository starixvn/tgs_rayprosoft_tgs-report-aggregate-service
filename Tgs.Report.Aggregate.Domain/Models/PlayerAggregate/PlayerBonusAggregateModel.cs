namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record PlayerBonusAggregateModel
{
    public Guid BrandId { get; set; }

    public Guid? AgentId { get; set; }

    public string PlayerCode { get; set; } = default!;

    public int Hour { get; set; }

    public DateTimeOffset ReportDate { get; set; }

    public DateTimeOffset ReportDateTime { get; set; }

    public decimal RebateAmount { get; set; }

    public decimal PromotionAmount { get; set; }

    public decimal UpgradeAmount { get; set; }

    public decimal SeasonAmount { get; set; }

    public decimal BirthdayAmount { get; set; }
}