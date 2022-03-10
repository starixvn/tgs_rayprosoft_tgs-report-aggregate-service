using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Entities.Configurations;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Repository.Repositories
{
    public class TgsRepository : DbContext, ITgsRepository
    {
        public TgsRepository(DbContextOptions<TgsRepository> options) : base(options)
        {
        }

        public DbSet<PlayerAggregate> PlayerAggregates { get; set; }

        public DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; }

        public DbSet<PlayerBucket> PlayerBuckets { get; set; }

        public DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<RoundAggregate> RoundAggregates { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<VipLevel> VipLevels { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<BrandGameProviderAggregate> BrandGameProviderAggregates { get; set; }

        public DbSet<GameProviderAggregate> GameProviderAggregates { get; set; }

        public DbSet<Round> Rounds { get; set; }

        public IDbContextTransaction? CurrentTransaction =>
            Database.CurrentTransaction;

        public IDbConnection GetDbConnection() =>
            Database.GetDbConnection();

        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel level) =>
            Database.BeginTransactionAsync(level);

        public Task<bool> CanConnectionAsync() => Database.CanConnectAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.HasPostgresExtension("pg_trgm");

            modelBuilder.ApplyConfiguration(new AgentConfiguration());
            modelBuilder.ApplyConfiguration(new BrandGameProviderAggregateConfiguration());
            modelBuilder.ApplyConfiguration(new GameProviderAggregateConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerAggregateConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerBucketConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerBucketHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new PlayerGameRebateAggregateConfiguration());
            modelBuilder.ApplyConfiguration(new RoundAggregateConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            modelBuilder.ApplyConfiguration(new VipLevelConfiguration());
            modelBuilder.ApplyConfiguration(new WalletConfiguration());
            modelBuilder.ApplyConfiguration(new RoundConfiguration());
        }
    }
}