using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class CreatePlayerAggregateTest
{
    [Test]
    public async Task Can_Create_Player_Aggregate()
    {
        var fakePlayerAggregateTaskQueue = Substitute.For<IPlayerAggregateTaskQueue>();
        var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();

        var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateTaskQueue, fakePlayerAggregateService);

        var playerAggregate = CreatePlayerAggregateFixture()
            .CreateMany<PlayerAggregate>(0)
            .AsQueryable()
            .BuildMock();

        _ = fakePlayerAggregateService.GetQueries().Returns(playerAggregate);

        var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

        Assert.That(response.Valid);
    }

    [Test]
    public async Task No_Create_Player_Aggregate_When_Round_Aggregate_Was_Generated()
    {
        var fakePlayerAggregateTaskQueue = Substitute.For<IPlayerAggregateTaskQueue>();
        var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();

        var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateTaskQueue, fakePlayerAggregateService);

        var playerAggregate = CreatePlayerAggregateFixture()
            .CreateMany<PlayerAggregate>(0)
            .AsQueryable()
            .BuildMock();

        var rounds = CreateRoundFixture()
            .CreateMany<Round>(DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0)).Hour + 1)
            .AsQueryable()
            .BuildMock();

        foreach (var round in rounds.Select((value, index) => new { value, index }))
            round.value.ClosedOn = new DateTimeOffset(
                DateTimeOffset.Now.Year,
                DateTimeOffset.Now.Month,
                DateTimeOffset.Now.Day,
                round.index,
                0,
                0,
                new TimeSpan(8, 0, 0)).UtcDateTime;

        _ = fakePlayerAggregateService.GetQueries().Returns(playerAggregate);

        var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

        Assert.That(response.Valid);
    }

    [Test]
    public async Task Can_Not_Create_Player_Aggregate_When_Player_Aggregate_Was_Generated()
    {
        var fakePlayerAggregateTaskQueue = Substitute.For<IPlayerAggregateTaskQueue>();
        var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();

        var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateTaskQueue, fakePlayerAggregateService);

        var previousHour = DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0)).AddHours(-1);
        var reportDate = new DateTimeOffset(previousHour.Year,
            previousHour.Month,
            previousHour.Day,
            0,
            0,
            0,
            previousHour.Offset);
        var hourArray = Enumerable.Range(0, previousHour.Hour + 1);

        var playerAggregate = hourArray
            .SelectMany(x => CreatePlayerAggregateWithDateFixture(reportDate, x)
                .CreateMany<PlayerAggregate>(1))
            .AsQueryable()
            .BuildMock();

        var rounds = CreateRoundFixture()
            .CreateMany<Round>(DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0)).Hour + 1)
            .AsQueryable()
            .BuildMock();

        foreach (var round in rounds.Select((value, index) => new { value, index }))
            round.value.ClosedOn = new DateTimeOffset(
                DateTimeOffset.Now.Year,
                DateTimeOffset.Now.Month,
                DateTimeOffset.Now.Day,
                round.index,
                0,
                0,
                new TimeSpan(8, 0, 0)).UtcDateTime;

        _ = fakePlayerAggregateService.GetQueries().Returns(playerAggregate);

        var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

        Assert.That(response.Valid, Is.False);
    }

    [Test]
    public async Task Can_publish_Vip_Aggregate()
    {
        var fakePlayerAggregateTaskQueue = Substitute.For<IPlayerAggregateTaskQueue>();
        var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();

        var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateTaskQueue, fakePlayerAggregateService);

        var playerAggregate = CreatePlayerAggregateFixture()
            .CreateMany<PlayerAggregate>(0)
            .AsQueryable()
            .BuildMock();

        _ = fakePlayerAggregateService.GetQueries().Returns(playerAggregate);

        var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

        Assert.That(response.Valid);
    }

    private static Fixture CreateRoundAggregateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<RoundAggregate>(u => u);
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateRoundFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Round>(u => u);
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerAggregateFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u);
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerAggregateWithDateFixture(DateTimeOffset? reportDate, int? hour)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(x => x.ReportDate, reportDate?.UtcDateTime)
            .With(x => x.Hour, hour));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}