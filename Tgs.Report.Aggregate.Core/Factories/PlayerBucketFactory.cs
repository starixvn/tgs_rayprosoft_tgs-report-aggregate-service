using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;
using Tgs.Report.Aggregate.Infrastructure.Configurations;
using Tgs.Report.Aggregate.Repository.Repositories;

namespace Tgs.Report.Aggregate.Core.Factories
{
    public class PlayerBucketFactory : IPlayerBucketFactory
    {
        private readonly ITgsRepository _repository;

        public PlayerBucketFactory(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async ValueTask FillInBucketAsync(FillInPlayerBucketsRequest request)
        {
            var playerCodes = request.PlayerTurnovers.Select(x => x.PlayerCode).ToArray();

            var playerBuckets = await _repository.PlayerBuckets
                .Where(x => playerCodes.Contains(x.PlayerCode) && !x.IsFull).ToArrayAsync();
            var playerBucketHistories = new List<PlayerBucketHistory>();

            foreach (var player in request.PlayerTurnovers)
            {
                var turnover = player.Turnover;
                var buckets = playerBuckets.Where(x => x.PlayerCode == player.PlayerCode).ToArray();

                foreach (var playerBucket in buckets)
                {
                    if (turnover <= 0)
                    {
                        break;
                    }

                    var amount = 0.0M;

                    if (turnover - (playerBucket.BucketAmount - playerBucket.CurrentBucketAmount) >= 0.0M)
                    {
                        amount = playerBucket.BucketAmount - playerBucket.CurrentBucketAmount;
                        turnover -= amount;
                        playerBucket.IsFull = true;
                        playerBucket.CurrentBucketAmount = playerBucket.BucketAmount;
                    }
                    else
                    {
                        amount = turnover;
                        playerBucket.CurrentBucketAmount += turnover;
                        turnover = 0;
                    }

                    playerBucketHistories.Add(new()
                    {
                        PlayerCode = player.PlayerCode,
                        Turnover = amount,
                        From = request.From.ToUniversalTime(),
                        To = request.To.ToUniversalTime(),
                        CreatedOn = DateTimeOffset.UtcNow
                    });
                }
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            _repository.PlayerBuckets.UpdateRange(playerBuckets);

            await _repository.PlayerBucketHistories.AddRangeAsync(playerBucketHistories);

            await _repository.SaveChangesAsync();

            scope.Complete();
        }
    }
}