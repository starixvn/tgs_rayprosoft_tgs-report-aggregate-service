using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Core.Factories;

public class RoundAggregateFactory(ITgsRepository repository) : IRoundAggregateFactory
{
    public async ValueTask<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start,
        DateTimeOffset end)
    {
        const string spName =
            "select * from dbo.report_generate_round_aggregates_xtest(@_from,@_to,@_report_date,@_created_on,@_hour,@_minute)";
        var conn = repository.GetDbConnection();
        var transaction = repository.CurrentTransaction?.GetDbTransaction();
        var result = await conn.QueryAsync<RoundAggregateModel>(spName,
            new Dictionary<string, object>
            {
                { "_from", start.ToUniversalTime() },
                { "_to", end.ToUniversalTime() },
                {
                    "_report_date",
                    new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset).ToUniversalTime()
                },
                { "_created_on", DateTimeOffset.UtcNow },
                { "_hour", start.Hour },
                { "_minute", start.Minute }
            },
            transaction);

        return result.ToArray();
    }
}