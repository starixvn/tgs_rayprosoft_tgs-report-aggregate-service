using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Entities;

namespace Tgs.Report.Aggregate.Domain.Interfaces;

public interface ITgsRepository
{
	IDbContextTransaction? CurrentTransaction { get; }

	DbSet<AdjustPlayerRecord> AdjustPlayerRecords { get; set; }

	DbSet<PlayerAggregate> PlayerAggregates { get; set; }

	DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; }

	DbSet<PlayerBucket> PlayerBuckets { get; set; }

	DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }

	DbSet<Player> Players { get; set; }

	DbSet<RoundAggregate> RoundAggregates { get; set; }

	DbSet<Transaction> Transactions { get; set; }

	DbSet<VipLevel> VipLevels { get; set; }

	DbSet<Wallet> Wallets { get; set; }

	DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; }

	DbSet<GameProviderAggregate> GameProviderAggregates { get; set; }

	DbSet<Round> Rounds { get; set; }

	IDbConnection GetDbConnection();

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	DbSet<T> Set<T>() where T : class;
}
