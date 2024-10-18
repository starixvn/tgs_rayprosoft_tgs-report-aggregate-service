using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Core.Commands.RoundAggregate;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class CreateRoundAggregateTest
{
    [Test]
    public async Task Can_Create_Round_Aggregate()
    {
        var fakeRoundAggregateFactory = Substitute.For<IRoundAggregateFactory>();
        var fakeAggregateExecutionTimeFactory = Substitute.For<IAggregateExecutionTimeFactory>();
        var fakeTgsRoundAggregateService = Substitute.For<ITgsService<RoundAggregate>>();

        var sut = new CreateRoundAggregateHandler(fakeTgsRoundAggregateService,
            fakeRoundAggregateFactory,
            fakeAggregateExecutionTimeFactory);

        var roundAggregate = CreateRoundAggregateFixture()
            .CreateMany<RoundAggregate>(0)
            .AsQueryable()
            .BuildMock();

        _ = fakeTgsRoundAggregateService.GetQueries().Returns(roundAggregate);

        _ = fakeAggregateExecutionTimeFactory.ExecutionTimeIsExistsAsync(Arg.Is(AggregateType.Round),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<DateTimeOffset>(),
                default)
            .Returns(false);

        var response = await sut.Handle(new CreateRoundAggregateRequest(), default);

        var count = DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0)).Hour == 0
            ? 24
            : DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0)).Hour;

        _ = await fakeRoundAggregateFactory.Received(count)
            .GenerateRoundAggregateAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());

        Assert.That(response.Valid);
    }

    private static Fixture CreateRoundAggregateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<RoundAggregate>(u => u);
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}