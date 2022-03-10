using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.MessageQueue;
using Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Models.PlayerGameAggregate;

namespace Tgs.Report.Aggregate.UnitTest
{
    public sealed class CreatePlayerAggregateTest
    {
        [Test]
        public async Task Can_Create_Player_Aggregate()
        {
            var fakeMessageSender = Substitute.For<IMessageSender>();
            var fakePlayerAggregateFactory = Substitute.For<IPlayerAggregateFactory>();
            var fakePlayerGameAggregateFactory = Substitute.For<IPlayerGameAggregateFactory>();
            var fakeRoundAggregateFactory = Substitute.For<IRoundAggregateFactory>();
            var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();
            var fakeRoundAggregateService = Substitute.For<ITgsService<RoundAggregate>>();
            var fakeRoundService = Substitute.For<ITgsService<Round>>();

            var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateService, fakeMessageSender,
                fakeRoundAggregateFactory, fakePlayerAggregateFactory, fakePlayerGameAggregateFactory,
                fakeRoundAggregateService, fakeRoundService);

            var playerAggregate = CreatePlayerAggregateFixture()
                .CreateMany<PlayerAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            var roundAggregate = CreateRoundAggregateFixture()
                .CreateMany<RoundAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            var round = CreateRoundFixture()
                .CreateMany<Round>(0)
                .AsQueryable()
                .BuildMock().Object;

            fakePlayerAggregateService.GetQueries().Returns(playerAggregate);
            fakeRoundAggregateService.GetQueries().Returns(roundAggregate);
            fakeRoundService.GetQueries().Returns(round);

            fakeRoundAggregateFactory.GetPlayerAggregatesAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                .Returns(ValueTask.FromResult((
                    CreatePlayerAggregateModelFixture().CreateMany<PlayerAggregateModel>(1).ToArray(),
                    CreatePlayerGameTypeAggregateModelFixture().CreateMany<PlayerGameTypeAggregateModel>(1)
                        .ToArray())));

            var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

            await fakePlayerAggregateFactory.Received(DateTimeOffset.Now.ToOffset(new(8, 0, 0)).Hour)
                .CreatePlayerAggregatesAsync(Arg.Any<PlayerAggregateModel[]>());

            await fakePlayerGameAggregateFactory.Received(DateTimeOffset.Now.ToOffset(new(8, 0, 0)).Hour)
                .CreatePlayerGameAggregatesAsync(Arg.Any<PlayerGameTypeAggregateModel[]>());

            Assert.IsTrue(response.Valid);
        }

        [Test]
        public async Task Can_Not_Create_Player_Aggregate_When_Round_Aggregate_Was_Generated()
        {
            var fakeMessageSender = Substitute.For<IMessageSender>();
            var fakePlayerAggregateFactory = Substitute.For<IPlayerAggregateFactory>();
            var fakePlayerGameAggregateFactory = Substitute.For<IPlayerGameAggregateFactory>();
            var fakeRoundAggregateFactory = Substitute.For<IRoundAggregateFactory>();
            var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();
            var fakeRoundAggregateService = Substitute.For<ITgsService<RoundAggregate>>();
            var fakeRoundService = Substitute.For<ITgsService<Round>>();

            var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateService, fakeMessageSender,
                fakeRoundAggregateFactory, fakePlayerAggregateFactory, fakePlayerGameAggregateFactory,
                fakeRoundAggregateService, fakeRoundService);

            var playerAggregate = CreatePlayerAggregateFixture()
                .CreateMany<PlayerAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            var roundAggregate = CreateRoundAggregateFixture()
                .CreateMany<RoundAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            var rounds = CreateRoundFixture()
                .CreateMany<Round>(DateTimeOffset.Now.ToOffset(new(8, 0, 0)).Hour + 1)
                .AsQueryable()
                .BuildMock().Object;

            foreach (var round in rounds.Select((value, index) => new { value, index }))
            {
                round.value.ClosedOn = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month,
                    DateTimeOffset.Now.Hour, round.index, 0, 0, new(8, 0, 0));
            }

            fakePlayerAggregateService.GetQueries().Returns(playerAggregate);
            fakeRoundAggregateService.GetQueries().Returns(roundAggregate);
            fakeRoundService.GetQueries().Returns(rounds);

            fakeRoundAggregateFactory.GetPlayerAggregatesAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                .Returns(ValueTask.FromResult((
                    CreatePlayerAggregateModelFixture().CreateMany<PlayerAggregateModel>(1).ToArray(),
                    CreatePlayerGameTypeAggregateModelFixture().CreateMany<PlayerGameTypeAggregateModel>(1)
                        .ToArray())));

            var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

            await fakePlayerAggregateFactory.Received(0)
                .CreatePlayerAggregatesAsync(Arg.Any<PlayerAggregateModel[]>());

            await fakePlayerGameAggregateFactory.Received(0)
                .CreatePlayerGameAggregatesAsync(Arg.Any<PlayerGameTypeAggregateModel[]>());

            Assert.IsTrue(response.Valid);
        }

        [Test]
        public async Task Can_publish_Vip_Aggregate()
        {
            var fakeMessageSender = Substitute.For<IMessageSender>();
            var fakePlayerAggregateFactory = Substitute.For<IPlayerAggregateFactory>();
            var fakePlayerGameAggregateFactory = Substitute.For<IPlayerGameAggregateFactory>();
            var fakeRoundAggregateFactory = Substitute.For<IRoundAggregateFactory>();
            var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();
            var fakeRoundAggregateService = Substitute.For<ITgsService<RoundAggregate>>();
            var fakeRoundService = Substitute.For<ITgsService<Round>>();

            var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateService, fakeMessageSender,
                fakeRoundAggregateFactory, fakePlayerAggregateFactory, fakePlayerGameAggregateFactory,
                fakeRoundAggregateService, fakeRoundService);

            var playerAggregate = CreatePlayerAggregateFixture()
                .CreateMany<PlayerAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            var roundAggregate = CreateRoundAggregateFixture()
                .CreateMany<RoundAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            var round = CreateRoundFixture()
                .CreateMany<Round>(1)
                .AsQueryable()
                .BuildMock().Object;

            fakePlayerAggregateService.GetQueries().Returns(playerAggregate);
            fakeRoundAggregateService.GetQueries().Returns(roundAggregate);
            fakeRoundService.GetQueries().Returns(round);

            fakeRoundAggregateFactory.GetPlayerAggregatesAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                .Returns(ValueTask.FromResult((
                    CreatePlayerAggregateModelFixture().CreateMany<PlayerAggregateModel>(1).ToArray(),
                    CreatePlayerGameTypeAggregateModelFixture().CreateMany<PlayerGameTypeAggregateModel>(1)
                        .ToArray())));

            var response = await sut.Handle(new CreateUpdatePlayerAggregateModel(), default);

            await fakeMessageSender.Received(1)
                .PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>());

            Assert.IsTrue(response.Valid);
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

        private static Fixture CreatePlayerAggregateModelFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregateModel>(u => u);
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerGameTypeAggregateModelFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerGameTypeAggregateModel>(u => u);
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}