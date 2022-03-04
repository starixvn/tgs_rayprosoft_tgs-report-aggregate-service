using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Aggregate.Core.Commands.RoundAggregate;
using Tgs.Report.Aggregate.Domain.Entities;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.UnitTest
{
    public sealed class CreateRoundAggregateTest
    {
        [Test]
        public async Task Can_Create_Round_Aggregate()
        {
            var fakeRoundFactory = Substitute.For<IRoundFactory>();
            var fakeTgsRoundAggregateService = Substitute.For<ITgsService<RoundAggregate>>();

            var sut = new CreateRoundAggregateHandler(fakeTgsRoundAggregateService, fakeRoundFactory);

            var roundAggregate = CreateRoundAggregateFixture()
                .CreateMany<RoundAggregate>(0)
                .AsQueryable()
                .BuildMock().Object;

            fakeTgsRoundAggregateService.GetQueries().Returns(roundAggregate);

            var response = await sut.Handle(new CreateRoundAggregateRequest(), default);

            await fakeRoundFactory.Received(DateTimeOffset.Now.ToOffset(new(8, 0, 0)).Hour)
                .GenerateRoundAggregateAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());

            Assert.IsTrue(response.Valid);
        }

        private static Fixture CreateRoundAggregateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<RoundAggregate>(u => u);
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}