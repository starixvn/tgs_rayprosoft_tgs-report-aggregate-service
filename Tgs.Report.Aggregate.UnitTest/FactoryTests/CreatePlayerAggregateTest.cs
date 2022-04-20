using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Core.Factories;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.UnitTest.FactoryTests
{
    public sealed class CreatePlayerAggregateTest
    {
        [Test]
        public async Task Can_Fill_In_Buckets()
        {
            var fakePlayerBucketHistoryService = Substitute.For<ITgsService<PlayerBucketHistory>>();
            var fakePlayerBucketService = Substitute.For<ITgsService<PlayerBucket>>();

            var sut = new PlayerBucketFactory(fakePlayerBucketService, fakePlayerBucketHistoryService);

            var roundMinutelyModels = CreateRoundMinutelyModelFixture("player", 100, 10)
                .CreateMany<RoundMinutelyModel>(1).Concat(CreateRoundMinutelyModelFixture("player", 100, 10)
                    .CreateMany<RoundMinutelyModel>(1)).AsQueryable()
                .BuildMock();

            var playerBuckets = CreatePlayerBucketFixture("player", 1000, 10)
                .CreateMany<PlayerBucket>(1).AsQueryable()
                .BuildMock();

            fakePlayerBucketService.GetQueries()
                .Returns(playerBuckets);

            await sut.FillInBucketAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                roundMinutelyModels.ToArray());

            await fakePlayerBucketService.Received(1)
                .UpdateEntitiesAsync(Arg.Is<PlayerBucket[]>(x => x.First().CurrentBucketAmount == 30));
        }

        [Test]
        public async Task Can_Create_Histories()
        {
            var fakePlayerBucketHistoryService = Substitute.For<ITgsService<PlayerBucketHistory>>();
            var fakePlayerBucketService = Substitute.For<ITgsService<PlayerBucket>>();

            var sut = new PlayerBucketFactory(fakePlayerBucketService, fakePlayerBucketHistoryService);

            var roundMinutelyModels = CreateRoundMinutelyModelFixture("player", 100, 10)
                .CreateMany<RoundMinutelyModel>(1).Concat(CreateRoundMinutelyModelFixture("player", 100, 10)
                    .CreateMany<RoundMinutelyModel>(1)).AsQueryable()
                .BuildMock();

            var playerBuckets = CreatePlayerBucketFixture("player", 1000, 10)
                .CreateMany<PlayerBucket>(1).AsQueryable()
                .BuildMock();

            fakePlayerBucketService.GetQueries()
                .Returns(playerBuckets);

            await sut.FillInBucketAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                roundMinutelyModels.ToArray());

            await fakePlayerBucketHistoryService.Received(1)
                .CreateEntitiesAsync(Arg.Is<PlayerBucketHistory[]>(x => x.Length == 1 &&
                    x.First().PlayerBucketId == playerBuckets.First().Id &&
                    x.First().Turnover == 20));
        }

        [Test]
        public async Task Can_Fill_In_Buckets_With_Same_Round()
        {
            var fakePlayerBucketHistoryService = Substitute.For<ITgsService<PlayerBucketHistory>>();
            var fakePlayerBucketService = Substitute.For<ITgsService<PlayerBucket>>();

            var sut = new PlayerBucketFactory(fakePlayerBucketService, fakePlayerBucketHistoryService);

            var roundMinutelyModels = CreateRoundMinutelyModelFixture("player", 800, 10)
                .CreateMany<RoundMinutelyModel>(1).AsQueryable()
                .BuildMock();

            var playerBuckets = CreatePlayerBucketFixture("player", 50, 0)
                .CreateMany<PlayerBucket>(2).AsQueryable()
                .BuildMock();

            fakePlayerBucketService.GetQueries()
                .Returns(playerBuckets);

            await sut.FillInBucketAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                roundMinutelyModels.ToArray());

            await fakePlayerBucketHistoryService.Received(1)
                .CreateEntitiesAsync(Arg.Is<PlayerBucketHistory[]>(x => x.Length == 2 &&
                    x[0].PlayerBucketId == playerBuckets.ToArray()[0].Id &&
                    x[0].Turnover == 50 &&
                    x[1].PlayerBucketId == playerBuckets.ToArray()[1].Id &&
                    x[1].Turnover == 30));
        }

        [Test]
        public async Task Can_Fill_In_Buckets_With_Multiple_Rounds()
        {
            var fakePlayerBucketHistoryService = Substitute.For<ITgsService<PlayerBucketHistory>>();
            var fakePlayerBucketService = Substitute.For<ITgsService<PlayerBucket>>();

            var sut = new PlayerBucketFactory(fakePlayerBucketService, fakePlayerBucketHistoryService);

            var roundMinutelyModels = CreateRoundMinutelyModelFixture("player", 800, 10)
                .CreateMany<RoundMinutelyModel>(2).AsQueryable()
                .BuildMock();

            var playerBuckets = CreatePlayerBucketFixture("player", 100, 0)
                .CreateMany<PlayerBucket>(2).AsQueryable()
                .BuildMock();

            fakePlayerBucketService.GetQueries()
                .Returns(playerBuckets);

            await sut.FillInBucketAsync(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                roundMinutelyModels.ToArray());

            await fakePlayerBucketHistoryService.Received(1)
                .CreateEntitiesAsync(Arg.Is<PlayerBucketHistory[]>(x => x.Length == 2 &&
                    x[0].PlayerBucketId == playerBuckets.ToArray()[0].Id &&
                    x[0].Turnover == 100 &&
                    x[1].PlayerBucketId == playerBuckets.ToArray()[1].Id &&
                    x[1].Turnover == 60));
        }


        private static Fixture CreateRoundMinutelyModelFixture(string playerCode, decimal turnover,
            decimal effectiveBettingPercentage)
        {
            var fixture = new Fixture();
            fixture.Customize<RoundMinutelyModel>(u => u
                .With(x => x.PlayerCode, playerCode)
                .With(x => x.Turnover, turnover)
                .With(x => x.EffectiveBettingPercentage, effectiveBettingPercentage));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerBucketFixture(string playerCode, decimal bucketAmount,
            decimal currentBucketAmount)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerBucket>(u => u
                .With(x => x.PlayerCode, playerCode)
                .With(x => x.BucketAmount, bucketAmount)
                .With(x => x.CurrentBucketAmount, currentBucketAmount)
                .With(x => x.IsFull, false));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}