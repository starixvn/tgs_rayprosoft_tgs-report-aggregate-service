using System.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Tgs.Report.Aggregate.Infrastructure.Configurations;

namespace Tgs.Report.Aggregate.Repository.Repositories
{
    public interface ITgsRepository
    {
        IDbContextTransaction? CurrentTransaction { get; }

        DbSet<PlayerAggregate> PlayerAggregates { get; set; }

        DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; }

        DbSet<PlayerBucket> PlayerBuckets { get; set; }

        DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }

        DbSet<Player> Players { get; set; }

        DbSet<RoundAggregate> RoundAggregates { get; set; }

        DbSet<Transaction> Transactions { get; set; }

        DbSet<VipLevel> VipLevels { get; set; }

        DbSet<Wallet> Wallets { get; set; }

        IDbConnection GetDbConnection();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        DbSet<T> Set<T>() where T : class;
    }
}
