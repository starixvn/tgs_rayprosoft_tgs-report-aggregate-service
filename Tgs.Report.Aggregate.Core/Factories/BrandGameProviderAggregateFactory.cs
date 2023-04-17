using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Factories;

public class BrandGameProviderAggregateFactory : IBrandGameProviderAggregateFactory
{
	private readonly ITgsRepository _repository;

	public BrandGameProviderAggregateFactory(ITgsRepository repository)
	{
		_repository = repository;
	}

	public async ValueTask GenerateBrandGameProviderAggregateAsync(Guid brandId, DateTimeOffset start,
		DateTimeOffset end, int auditLogId)
	{
		const string spName = "select * from dbo.report_generate_brand_game_provider_aggregate_by_closed_on(@_from,@_to,@_brand_id,@_audit_log_id,@_created_on)";
		var conn = _repository.GetDbConnection();
		var transaction = _repository.CurrentTransaction?.GetDbTransaction();
		_ = await conn.ExecuteAsync(
			spName,
			new Dictionary<string, object>
			{
				{ "_from", start.ToUniversalTime() },
				{ "_to", end.ToUniversalTime() },
				{ "_brand_id", brandId },
				{ "_audit_log_id", auditLogId },
				{ "_created_on", DateTimeOffset.UtcNow }
			},
			transaction: transaction).ConfigureAwait(false);
	}

	public async ValueTask<int> RemoveBrandGameProviderAggregateAsync(Guid? brandId, DateTimeOffset start,
		DateTimeOffset end)
	{
		var data = _repository.BrandGameProviderAggregates
			.Where(x => x.ReportDate >= start.ToUniversalTime() && x.ReportDate < end.ToUniversalTime())
			.Where(brandId, x => x.BrandId == brandId);

		_repository.BrandGameProviderAggregates.RemoveRange(data);

		return await _repository.SaveChangesAsync().ConfigureAwait(false);
	}
}
