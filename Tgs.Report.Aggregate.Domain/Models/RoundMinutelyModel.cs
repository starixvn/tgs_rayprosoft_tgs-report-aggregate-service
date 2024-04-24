namespace Tgs.Report.Aggregate.Domain.Models;

public record RoundMinutelyModel
{
    public Guid RoundId { get; set; }

    public string PlayerCode { get; set; } = default!;

    public decimal Turnover { get; set; }

    public decimal EffectiveBettingPercentage { get; set; }
}