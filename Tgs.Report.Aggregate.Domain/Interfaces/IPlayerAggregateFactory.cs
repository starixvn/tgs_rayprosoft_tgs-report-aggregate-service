using System.Threading.Tasks;

using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IPlayerAggregateFactory
{
	Task CreatePlayerAggregatesAsync(PlayerAggregateModel[] request);
}
