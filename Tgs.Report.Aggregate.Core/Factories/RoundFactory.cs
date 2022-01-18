using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;
using Tgs.Report.Aggregate.Repository.Repositories;

namespace Tgs.Report.Aggregate.Core.Factories
{
    public class RoundFactory : IRoundFactory
    {
        private readonly ITgsRepository _repository;

        public RoundFactory(ITgsRepository repository)
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
                    { "_from", start },
                    { "_to", end },
                    {
                        "_report_date",
                        new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset)
                    },
                    { "_created_on", DateTimeOffset.Now.ToOffset(start.Offset) },
                    { "_hour", start.Hour },
                    { "_minute", start.Minute }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);

            return result.ToArray();
        }

        public async ValueTask<RoundAggregateModel[]> GetMinuteRoundAggregateAsync(DateTimeOffset start,
            DateTimeOffset end)
        {
            const string spName = "dbo.report_get_round_and_set_game_setting_aggregates";
            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
            var result = await conn.QueryAsync<RoundAggregateModel>(spName,
                new Dictionary<string, object>
                {
                    { "_from", start },
                    { "_to", end },
                    {
                        "_report_date",
                        new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset)
                    },
                    { "_hour", start.Hour },
                    { "_minute", start.Minute }
                }, commandType: CommandType.StoredProcedure, transaction: transaction);

            return result.ToArray();
        }
    }
}