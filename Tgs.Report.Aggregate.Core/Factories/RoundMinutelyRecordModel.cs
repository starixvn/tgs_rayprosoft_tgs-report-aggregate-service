namespace Tgs.Report.Aggregate.Core.Factories;

public class RoundMinutelyRecordModel
{
    public Guid RoundId { get; set; }

    public string PlayerCode { get; set; } = default!;

    public decimal OriginalTurnover { get; set; }

    public decimal Turnover { get; set; }

    public decimal EffectiveBettingPercentage { get; set; }

    public decimal RemainingTurnover { get; set; }
}