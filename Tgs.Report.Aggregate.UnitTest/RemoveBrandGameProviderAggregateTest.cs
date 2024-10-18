using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using StackExchange.Redis;
using Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class RemoveBrandGameProviderAggregateTest
{
    [Test]
    public async Task Have_Create_Brand_Game_Provider_Aggregate()
    {
        var fakeBrandGameProviderAggregateFactory = Substitute.For<IBrandGameProviderAggregateFactory>();
        var fakeDataBase = Substitute.For<IDatabase>();

        var sut = new RemoveBrandGameProviderAggregateHandler(fakeBrandGameProviderAggregateFactory);

        _ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(false);

        var request = new RemoveBrandGameProviderAggregateRequest(Guid.NewGuid(),
            new DateTimeOffset(2022, 2, 1, 0, 0, 0, new TimeSpan(8, 0, 0)),
            new DateTimeOffset(2022, 3, 1, 0, 0, 0, new TimeSpan(8, 0, 0)));

        var response = await sut.Handle(request, default);

        _ = await fakeBrandGameProviderAggregateFactory.Received(1)
            .RemoveBrandGameProviderAggregateAsync(Arg.Any<Guid>(),
                Arg.Is(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new TimeSpan(8, 0, 0))),
                Arg.Is(new DateTimeOffset(2022, 3, 1, 0, 0, 0, new TimeSpan(8, 0, 0))));

        Assert.That(response.Valid);
    }
}