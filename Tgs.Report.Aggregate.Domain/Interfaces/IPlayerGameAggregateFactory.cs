using System.Threading.Tasks;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.Domain.Interfaces
{
    public interface IPlayerGameAggregateFactory
    {
        Task CreatePlayerGameAggregatesAsync(PlayerGameTypeAggregateModel[] request);
    }
}