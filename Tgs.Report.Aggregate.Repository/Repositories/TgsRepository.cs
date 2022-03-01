using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Entities.Configurations;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Tgs.Report.Aggregate.Repository.Repositories
{
    public class TgsRepository : TgsContext, ITgsRepository
    {
        public TgsRepository(DbContextOptions<TgsRepository> options)
        {
        }

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
        }
    }
}