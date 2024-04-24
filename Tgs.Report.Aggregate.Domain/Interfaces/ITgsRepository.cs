using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Entities;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface ITgsRepository
{
    IDbContextTransaction? CurrentTransaction { get; }

    DbSet<AdjustPlayerRecord> AdjustPlayerRecords { get; set; }

    DbSet<PlayerAggregate> PlayerAggregates { get; set; }

    DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }

    DbSet<Player> Players { get; set; }

    DbSet<RoundAggregate> RoundAggregates { get; set; }

    DbSet<Transaction> Transactions { get; set; }

    DbSet<VipLevel> VipLevels { get; set; }

    DbSet<Wallet> Wallets { get; set; }

    DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; }

    DbSet<GameProviderAggregate> GameProviderAggregates { get; set; }

    DbSet<Round> Rounds { get; set; }

    DbSet<PlayerBonusAggregate> PlayerBonusAggregates { get; set; }

    DbSet<AggregateExecutionTime> AggregateExecutionTimes { get; set; }

    DbSet<Agent> Agents { get; set; }

    DbSet<AgentProductFee> AgentProductFees { get; set; }

    IDbConnection GetDbConnection();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    DbSet<T> Set<T>() where T : class;

    ChangeTracker GetChangeTracker();
}