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

            var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateService, fakeMessageSender,
                fakeRoundAggregateFactory, fakePlayerAggregateFactory, fakePlayerGameAggregateFactory);

            var playerAggregate = CreatePlayerAggregateFixture()
                .CreateMany<PlayerAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            fakePlayerAggregateService.GetQueries().Returns(playerAggregate);

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
        public async Task Can_publish_Vip_Aggregate()
        {
            var fakeMessageSender = Substitute.For<IMessageSender>();
            var fakePlayerAggregateFactory = Substitute.For<IPlayerAggregateFactory>();
            var fakePlayerGameAggregateFactory = Substitute.For<IPlayerGameAggregateFactory>();
            var fakeRoundAggregateFactory = Substitute.For<IRoundAggregateFactory>();
            var fakePlayerAggregateService = Substitute.For<ITgsService<PlayerAggregate>>();

            var sut = new CreateUpdatePlayerAggregateHandler(fakePlayerAggregateService, fakeMessageSender,
                fakeRoundAggregateFactory, fakePlayerAggregateFactory, fakePlayerGameAggregateFactory);

            var playerAggregate = CreatePlayerAggregateFixture()
                .CreateMany<PlayerAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            fakePlayerAggregateService.GetQueries().Returns(playerAggregate);

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