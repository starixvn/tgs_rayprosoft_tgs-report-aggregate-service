using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using StackExchange.Redis;
using Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.UnitTest;

public sealed class CreateBrandGameProviderAggregateTest
{
	[Test]
	public async Task No_Create_Brand_Game_Provider_Aggregate()
	{
		var fakeBrandGameProviderAggregateFactory = Substitute.For<IBrandGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateBrandGameProviderAggregateHandler(fakeBrandGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(false);

		var request = new CreateBrandGameProviderAggregateRequest(DateTimeOffset.Now, Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		await fakeBrandGameProviderAggregateFactory.Received(0)
			.GenerateBrandGameProviderAggregateAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(),
				Arg.Any<DateTimeOffset>(), Arg.Any<int>());

		Assert.IsTrue(response.Valid);
	}

	[Test]
	public async Task Can_Create_Brand_Game_Provider_Aggregate()
	{
		var fakeBrandGameProviderAggregateFactory = Substitute.For<IBrandGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateBrandGameProviderAggregateHandler(fakeBrandGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeRedisConnectionFactory.GetDataBase().Returns(fakeDataBase);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(true);

		var request = new CreateBrandGameProviderAggregateRequest(DateTimeOffset.Now, Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		await fakeBrandGameProviderAggregateFactory.Received(1)
			.GenerateBrandGameProviderAggregateAsync(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(),
				Arg.Any<DateTimeOffset>(), Arg.Any<int>());

		Assert.IsTrue(response.Valid);
	}

	[Test]
	public async Task Can_Remove_Brand_Game_Provider_Redis_Key()
	{
		var fakeBrandGameProviderAggregateFactory = Substitute.For<IBrandGameProviderAggregateFactory>();
		var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
		var fakeDataBase = Substitute.For<IDatabase>();

		var sut = new CreateBrandGameProviderAggregateHandler(fakeBrandGameProviderAggregateFactory,
			fakeRedisConnectionFactory);

		_ = fakeRedisConnectionFactory.GetDataBase().Returns(fakeDataBase);

		_ = fakeDataBase.SetContainsAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>()).Returns(true);

		var request = new CreateBrandGameProviderAggregateRequest(DateTimeOffset.Now, Guid.NewGuid(),
			1, "redisKey");

		var response = await sut.Handle(request, default);

		_ = await fakeDataBase.Received(1)
			.SetRemoveAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>());

		Assert.IsTrue(response.Valid);
	}
}
