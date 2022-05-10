using System;
using System.Threading.Tasks;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IGameProviderAggregateFactory
{
	ValueTask GenerateGameProviderAggregateAsync(Guid gameProviderId, DateTimeOffset start, DateTimeOffset end,
		int auditLogId);

	ValueTask<int> RemoveGameProviderAggregateAsync(Guid? brandId, DateTimeOffset start, DateTimeOffset end);
}
