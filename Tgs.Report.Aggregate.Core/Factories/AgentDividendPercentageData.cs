namespace Tgs.Report.Aggregate.Core.Factories
{
    public class AgentDividendPercentageData
    {
        public Guid AgentId {get; set; }
        public Guid? ParentAgentId {get;set;}
        public decimal DividendPercentage {get; set; }
    }
}
