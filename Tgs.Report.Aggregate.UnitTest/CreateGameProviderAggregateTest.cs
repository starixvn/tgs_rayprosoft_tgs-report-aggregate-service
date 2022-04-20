using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using StackExchange.Redis;
using Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class CreateGameProviderAggregateTest
{
	[Test]
	public async Task No_Create_Game_Provider_Aggregate()
	{
		var fakeGameProviderAggregateFactory = Substitute.For<IGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateGameProviderAggregateHandler(fakeGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(false);

		var request = new CreateGameProviderAggregateRequest(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new(8, 0, 0)),
			Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		await fakeGameProviderAggregateFactory.Received(0)
			.GenerateGameProviderAggregateAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(),
				Arg.Any<DateTimeOffset>(), Arg.Any<int>());

		Assert.IsTrue(response.Valid);
	}

	[Test]
	public async Task Can_Create_Game_Provider_Aggregate()
	{
		var fakeGameProviderAggregateFactory = Substitute.For<IGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateGameProviderAggregateHandler(fakeGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeRedisConnectionFactory.GetDataBase().Returns(fakeDataBase);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(true);

		var request = new CreateGameProviderAggregateRequest(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new(8, 0, 0)),
			Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		await fakeGameProviderAggregateFactory.Received(1)
			.GenerateGameProviderAggregateAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(),
				Arg.Any<DateTimeOffset>(), Arg.Any<int>());

		Assert.IsTrue(response.Valid);
	}

	[Test]
	public async Task Can_Create_Game_Provider_Aggregate_With_Right_Date()
	{
		var fakeGameProviderAggregateFactory = Substitute.For<IGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateGameProviderAggregateHandler(fakeGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeRedisConnectionFactory.GetDataBase().Returns(fakeDataBase);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(true);

		var request = new CreateGameProviderAggregateRequest(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new(8, 0, 0)),
			Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		await fakeGameProviderAggregateFactory.Received(1)
			.GenerateGameProviderAggregateAsync(Arg.Any<Guid>(),
				Arg.Is(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new(8, 0, 0))),
				Arg.Is(new DateTimeOffset(2022, 3, 1, 0, 0, 0, new(8, 0, 0))), Arg.Any<int>());

		Assert.IsTrue(response.Valid);
	}

	[Test]
	public async Task Can_Remove_Game_Provider_Redis_Key()
	{
		var fakeGameProviderAggregateFactory = Substitute.For<IGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateGameProviderAggregateHandler(fakeGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeRedisConnectionFactory.GetDataBase().Returns(fakeDataBase);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(true);

		var request = new CreateGameProviderAggregateRequest(new DateTimeOffset(2022, 2, 1, 0, 0, 0, new(8, 0, 0)),
			Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		_ = await fakeDataBase.Received(1)
			.SetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>());

		Assert.IsTrue(response.Valid);
	}
}
