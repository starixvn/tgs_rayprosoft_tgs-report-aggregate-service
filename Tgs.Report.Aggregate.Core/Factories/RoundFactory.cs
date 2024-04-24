using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Core.Factories;

public class RoundFactory(ITgsRepository repository) : IRoundFactory
{
    public async ValueTask<RoundMinutelyModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start,
        DateTimeOffset end)
    {
        const string spName = "select * from dbo.report_get_round_and_set_game_setting_aggregates(@_from, @_to)";
        var conn = repository.GetDbConnection();
        var transaction = repository.CurrentTransaction?.GetDbTransaction();
        var result = await conn.QueryAsync<RoundMinutelyModel>(spName,
            new Dictionary<string, object>
            {
                { "_from", start.ToUniversalTime() },
                { "_to", end.ToUniversalTime() }
            },
            transaction);

        return result.ToArray();
    }
}