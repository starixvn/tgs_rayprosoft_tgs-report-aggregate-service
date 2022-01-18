using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Infrastructure.Configurations;

namespace Tgs.Report.Aggregate.Repository.Repositories
{
    public interface ITgsRepository
    {
        DbSet<PlayerAggregate> PlayerAggregates { get; set; }
        DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }
        DbSet<Player> Players { get; set; }
        DbSet<RoundAggregate> RoundAggregates { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<PlayerBucket> PlayerBuckets { get; set; }
        DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; }
        DbSet<VipLevel> VipLevels { get; set; }
        DbSet<Wallet> Wallets { get; set; }

        IDbContextTransaction? CurrentTransaction { get; }
        DbSet<T> Set<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        IDbConnection GetDbConnection();
    }
}