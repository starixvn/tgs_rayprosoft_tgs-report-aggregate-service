using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class CreateGameProviderAggregateTest
{
    [Test]
    public async Task Can_Create_Game_Provider_Aggregate_In_Queue()
    {
        var fakeTaskQueue = Substitute.For<IGameProviderAggregateTaskQueue>();

        var sut = new CreateGameProviderAggregateHandler(fakeTaskQueue);

        var request = new CreateGameProviderAggregateRequest(new(2022, 2, 1, 0, 0, 0, new(8, 0, 0)),
            Guid.NewGuid(),
            1, "redisKey");

        var response = await sut.Handle(request, default);

        await fakeTaskQueue.Received(1)
            .QueueGameProviderAggregateAsync(Arg.Any<GameProviderAggregateQueueModel>());

        Assert.IsTrue(response.Valid);
    }
}