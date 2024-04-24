using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Core.Factories;

public class GameProviderAggregateFactory(ITgsRepository repository) : IGameProviderAggregateFactory
{
    public async ValueTask GenerateGameProviderAggregateAsync(Guid gameProviderId,
        DateTimeOffset start,
        DateTimeOffset end,
        int auditLogId)
    {
        const string spName =
            "select * from dbo.report_generate_game_provider_aggregate_by_external_closed_on(@_from,@_to,@_game_provider_id,@_audit_log_id,@_created_on)";
        var conn = repository.GetDbConnection();
        var transaction = repository.CurrentTransaction?.GetDbTransaction();
        _ = await conn.ExecuteAsync(spName,
                new Dictionary<string, object>
                {
                    { "_from", start.ToUniversalTime() },
                    { "_to", end.ToUniversalTime() },
                    { "_game_provider_id", gameProviderId },
                    { "_audit_log_id", auditLogId },
                    { "_created_on", DateTimeOffset.UtcNow }
                },
                transaction)
            .ConfigureAwait(false);
    }

    public async ValueTask<int> RemoveGameProviderAggregateAsync(Guid? GameProviderId,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var data = repository.GameProviderAggregates
            .Where(x => x.ReportDate >= start.ToUniversalTime() && x.ReportDate < end.ToUniversalTime())
            .Where(GameProviderId, x => x.GameProviderId == GameProviderId);

        repository.GameProviderAggregates.RemoveRange(data);

        return await repository.SaveChangesAsync().ConfigureAwait(false);
    }
}