using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Core.Factories;

public class RoundFactory : IRoundFactory
{
	private readonly ITgsRepository _repository;

	public RoundFactory(ITgsRepository repository)
	{
		_repository = repository;
	}

	public async ValueTask<RoundMinutelyModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start,
		DateTimeOffset end)
	{
		const string spName = "dbo.report_get_round_and_set_game_setting_aggregates";
		var conn = _repository.GetDbConnection();
		var transaction = _repository.CurrentTransaction?.GetDbTransaction();
		var result = await conn.QueryAsync<RoundMinutelyModel>(spName,
			new Dictionary<string, object>
			{
					{ "_from", start.ToUniversalTime() },
					{ "_to", end.ToUniversalTime() }
			}, commandType: CommandType.StoredProcedure, transaction: transaction);

		return result.ToArray();
	}
}
