using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Entities.Configurations;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Repository.Repositories;

public class TgsRepository : DbContext, ITgsRepository
{
	public DbSet<AdjustPlayerRecord> AdjustPlayerRecords { get; set; } = default!;

	public DbSet<PlayerAggregate> PlayerAggregates { get; set; } = default!;

	public DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; } = default!;

	public DbSet<PlayerBucket> PlayerBuckets { get; set; } = default!;

	public DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; } = default!;

	public DbSet<Player> Players { get; set; } = default!;

	public DbSet<RoundAggregate> RoundAggregates { get; set; } = default!;

	public DbSet<Transaction> Transactions { get; set; } = default!;

	public DbSet<VipLevel> VipLevels { get; set; } = default!;

	public DbSet<Wallet> Wallets { get; set; } = default!;

	public DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; } = default!;

	public DbSet<GameProviderAggregate> GameProviderAggregates { get; set; } = default!;

	public DbSet<Round> Rounds { get; set; } = default!;
    public DbSet<PlayerBonusAggregate> PlayerBonusAggregates { get; set; } = null!;
    public DbSet<AggregateExecutionTime> AggregateExecutionTimes { get; set; } = null!;

	public IDbContextTransaction? CurrentTransaction =>
		Database.CurrentTransaction;

	public TgsRepository(DbContextOptions<TgsRepository> options) : base(options)
	{
	}

	public IDbConnection GetDbConnection() =>
		Database.GetDbConnection();

	public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel level) =>
		Database.BeginTransactionAsync(level);

	public Task<bool> CanConnectionAsync() => Database.CanConnectAsync();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		_ = modelBuilder.HasDefaultSchema("dbo");
		_ = modelBuilder.HasPostgresExtension("pg_trgm");

		_ = modelBuilder.ApplyConfiguration(new AdjustPlayerRecordConfiguration());
		_ = modelBuilder.ApplyConfiguration(new AgentConfiguration());
		_ = modelBuilder.ApplyConfiguration(new BrandGameProviderAggregateConfiguration());
		_ = modelBuilder.ApplyConfiguration(new GameProviderAggregateConfiguration());
		_ = modelBuilder.ApplyConfiguration(new PlayerConfiguration());
		_ = modelBuilder.ApplyConfiguration(new PlayerAggregateConfiguration());
		_ = modelBuilder.ApplyConfiguration(new PlayerBucketConfiguration());
		_ = modelBuilder.ApplyConfiguration(new PlayerBucketHistoryConfiguration());
		_ = modelBuilder.ApplyConfiguration(new PlayerGameRebateAggregateConfiguration());
		_ = modelBuilder.ApplyConfiguration(new RoundAggregateConfiguration());
		_ = modelBuilder.ApplyConfiguration(new TransactionConfiguration());
		_ = modelBuilder.ApplyConfiguration(new VipLevelConfiguration());
		_ = modelBuilder.ApplyConfiguration(new WalletConfiguration());
		_ = modelBuilder.ApplyConfiguration(new RoundConfiguration());
        _ = modelBuilder.ApplyConfiguration(new PlayerBonusAggregateConfiguration());
        _ = modelBuilder.ApplyConfiguration(new AggregateExecutionTimeConfiguration());
	}
}
