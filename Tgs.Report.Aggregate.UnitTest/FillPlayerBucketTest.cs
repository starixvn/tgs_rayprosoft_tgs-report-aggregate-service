using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class FillPlayerBucketTest
{
    [Test]
    public async Task Fill_Player_Bucket_Enqueue()
    {
        var fakeFillPlayerBucketTaskQueue = Substitute.For<IFillPlayerBucketTaskQueue>();
        var fakeRoundFactory = Substitute.For<IRoundFactory>();
        var fakeLogger = Substitute.For<ILogger<FillPlayerBucketMinutelyHandler>>();

        var sut = new FillPlayerBucketMinutelyHandler(fakeFillPlayerBucketTaskQueue, fakeRoundFactory, fakeLogger);

        var models = new RoundMinutelyModel[]
        {
            new()
            {
                RoundId = Guid.NewGuid(),
                PlayerCode = "playerCode",
                Turnover = 100M
            }
        };

        _ = fakeRoundFactory.GetMinuteRoundAggregateAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(models);

        var request = new FillPlayerBucketMinutelyRequest(DateTimeOffset.UtcNow);

        _ = await sut.Handle(request, default);

        await fakeFillPlayerBucketTaskQueue.Received(1)
            .QueueFillPlayerBucketAsync(
                Arg.Any<FillPlayerBucketQueueModel>());
    }

    [Test]
    public async Task Fill_Player_Bucket_When_70_Percentage()
    {
        var fakeFillPlayerBucketTaskQueue = Substitute.For<IFillPlayerBucketTaskQueue>();
        var fakeRoundFactory = Substitute.For<IRoundFactory>();
        var fakeLogger = Substitute.For<ILogger<FillPlayerBucketMinutelyHandler>>();

        var sut = new FillPlayerBucketMinutelyHandler(fakeFillPlayerBucketTaskQueue, fakeRoundFactory, fakeLogger);

        var models = new RoundMinutelyModel[]
        {
            new()
            {
                RoundId = Guid.NewGuid(),
                PlayerCode = "playerCode",
                Turnover = 20,
                EffectiveBettingPercentage = 70
            }
        };

        _ = fakeRoundFactory.GetMinuteRoundAggregateAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
            .Returns(models);

        var request = new FillPlayerBucketMinutelyRequest(DateTimeOffset.UtcNow);

        _ = await sut.Handle(request, default);

        await fakeFillPlayerBucketTaskQueue.Received(1)
            .QueueFillPlayerBucketAsync(
                Arg.Is<FillPlayerBucketQueueModel>(x => x.Turnover == 14));
    }
}