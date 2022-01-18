using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Infrastructure.Configurations;

namespace Tgs.Report.Aggregate.Repository.Repositories
{
    public class TgsRepository : DbContext, ITgsRepository
    {
        public DbSet<Agent> Agents { get; set; } = null!;

        public TgsRepository(DbContextOptions<TgsRepository> options) : base(options)
        {
        }

        public IDbContextTransaction? CurrentTransaction =>
            Database.CurrentTransaction;

        public DbSet<PlayerAggregate> PlayerAggregates { get; set; } = null!;
        public DbSet<PlayerGameRebateAggregate> PlayerGameRebateAggregates { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<RoundAggregate> RoundAggregates { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<PlayerBucket> PlayerBuckets { get; set; } = null!;
        public DbSet<PlayerBucketHistory> PlayerBucketHistories { get; set; } = null!;
        public DbSet<VipLevel> VipLevels { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;

        public IDbConnection GetDbConnection() =>
            Database.GetDbConnection();

        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel level) =>
            Database.BeginTransactionAsync(level);

        public Task<bool> CanConnectionAsync() => Database.CanConnectAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            modelBuilder.HasPostgresExtension("pg_trgm");
        }
    }
}