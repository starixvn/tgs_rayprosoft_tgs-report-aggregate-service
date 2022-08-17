using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.Core.Factories;

public class RoundAggregateFactory : IRoundAggregateFactory
{
    private readonly ITgsRepository _repository;

    public RoundAggregateFactory(ITgsRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<RoundAggregateModel[]> GenerateRoundAggregateAsync(DateTimeOffset start,
        DateTimeOffset end)
    {
        const string spName = "dbo.report_generate_round_aggregates";
        var conn = _repository.GetDbConnection();
        var transaction = _repository.CurrentTransaction?.GetDbTransaction();
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
            }, commandType: CommandType.StoredProcedure, transaction: transaction);

        return result.ToArray();
    }
}
