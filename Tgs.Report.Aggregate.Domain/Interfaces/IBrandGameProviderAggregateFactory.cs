using System;
using System.Threading.Tasks;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface IBrandGameProviderAggregateFactory
{
	ValueTask GenerateBrandGameProviderAggregateAsync(Guid brandId, DateTimeOffset start, DateTimeOffset end,
		int auditLogId);

	ValueTask<int> RemoveBrandGameProviderAggregateAsync(Guid? brandId, DateTimeOffset start, DateTimeOffset end);
}
