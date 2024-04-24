using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using StackExchange.Redis;
using Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class RemoveGameProviderAggregateTest
{
    [Test]
    public async Task Have_Remove_Game_Provider_Aggregate()
    {
        var fakeGameProviderAggregateFactory = Substitute.For<IGameProviderAggregateFactory>();
        var fakeDataBase = Substitute.For<IDatabase>();

        var sut = new RemoveGameProviderAggregateHandler(fakeGameProviderAggregateFactory);

        _ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(false);

        var request = new RemoveGameProviderAggregateRequest(Guid.NewGuid(),
            new DateTimeOffset(2022, 2, 1, 0, 0, 0, new TimeSpan(8, 0, 0)));

        var response = await sut.Handle(request, default);

        _ = await fakeGameProviderAggregateFactory.Received(1)
            .RemoveGameProviderAggregateAsync(Arg.Any<Guid>(),
                Arg.Is(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new TimeSpan(8, 0, 0))),
                Arg.Is(new DateTimeOffset(2022, 3, 1, 0, 0, 0, new TimeSpan(8, 0, 0))));

        Assert.IsTrue(response.Valid);
    }
}