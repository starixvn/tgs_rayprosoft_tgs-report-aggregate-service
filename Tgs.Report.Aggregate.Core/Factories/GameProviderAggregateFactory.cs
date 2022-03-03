using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Factories
{
    public class GameProviderAggregateFactory : IGameProviderAggregateFactory
    {
        private readonly ITgsRepository _repository;

        public GameProviderAggregateFactory(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask GenerateGameProviderAggregateAsync(Guid gameProviderId, DateTimeOffset start,
            DateTimeOffset end, int auditLogId)
        {
            const string spName = "dbo.report_generate_game_provider_aggregate_by_external_closed_on";
            var conn = _repository.GetDbConnection();
            var transaction = _repository.CurrentTransaction?.GetDbTransaction();
            await conn.ExecuteAsync(spName,
                new Dictionary<string, object>
                {
                    { "_from", start.ToUniversalTime() },
                    { "_to", end.ToUniversalTime() },
                    { "_game_provider_id", gameProviderId },
                    { "_audit_log_id", auditLogId },
                    { "_created_on", DateTimeOffset.UtcNow }
                }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);
        }

        public async ValueTask<int> RemoveGameProviderAggregateAsync(Guid? GameProviderId, DateTimeOffset start,
            DateTimeOffset end)
        {
            var data = _repository.GameProviderAggregates
                .Where(x => x.ReportDate >= start.ToUniversalTime() && x.ReportDate < end.ToUniversalTime())
                .Where(GameProviderId, x => x.GameProviderId == GameProviderId);

            _repository.GameProviderAggregates.RemoveRange(data);

            return await _repository.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}