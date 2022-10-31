using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Factories;

public class AggregateExecutionTimeFactory : IAggregateExecutionTimeFactory
{
	private readonly ITgsRepository _repository;

	public AggregateExecutionTimeFactory(ITgsRepository repository)
	{
		_repository = repository;
	}

	public async ValueTask<int> CreateExecutionTimeAsync(AggregateType type, DateTimeOffset from, DateTimeOffset to,
		CancellationToken cancellationToken)
	{
		_ = await _repository.AggregateExecutionTimes.AddAsync(new()
		{
			AggregateType = (short)type,
			From = from.ToUniversalTime(),
			To = to.ToUniversalTime()
		}, cancellationToken);

		return await _repository.SaveChangesAsync(cancellationToken);
	}

	public async ValueTask<bool> ExecutionTimeIsExistsAsync(AggregateType type, DateTimeOffset from, DateTimeOffset to,
		CancellationToken cancellationToken)
		=> await _repository.AggregateExecutionTimes.AnyAsync(
			x => x.AggregateType == (short)type && x.From == from.ToUniversalTime() && x.To == to.ToUniversalTime(),
			cancellationToken);
}