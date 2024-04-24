namespace Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

public record AgentDividendPercentageModel
{
    public Guid AgentId { get; set; }
    public Guid? ParentAgentId { get; set; }
    public decimal DividendPercentage { get; set; }
}