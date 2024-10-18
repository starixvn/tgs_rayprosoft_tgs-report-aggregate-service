using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class CreateBrandGameProviderAggregateTest
{
    [Test]
    public async Task No_Create_Brand_Game_Provider_Aggregate_In_Queue()
    {
        var fakeTaskQueue = Substitute.For<IBrandGameProviderAggregateTaskQueue>();

        var sut = new CreateBrandGameProviderAggregateHandler(fakeTaskQueue);

        var request = new CreateBrandGameProviderAggregateRequest(DateTimeOffset.Now,
            Guid.NewGuid(),
            1,
            "redisKey");

        var response = await sut.Handle(request, default);

        await fakeTaskQueue.Received(1)
            .QueueBrandGameProviderAggregateAsync(Arg.Any<BrandGameProviderAggregateQueueModel>());

        Assert.That(response.Valid);
    }
}