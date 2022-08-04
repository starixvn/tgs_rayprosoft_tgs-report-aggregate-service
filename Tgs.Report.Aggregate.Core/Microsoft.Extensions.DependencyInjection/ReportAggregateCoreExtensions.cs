using System.Reflection;
using MediatR;
using Tgs.Report.Aggregate.Core.Factories;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class ReportAggregateCoreExtensions
{
	public static IServiceCollection AddReportAggregateCore(this IServiceCollection services)
	{
		_ = services.AddMediatR(Assembly.GetExecutingAssembly());
		_ = services.AddTransient<IPlayerBucketFactory, PlayerBucketFactory>();
		_ = services.AddTransient<IPlayerGameAggregateFactory, PlayerGameAggregateFactory>();
		_ = services.AddTransient<IRoundAggregateFactory, RoundAggregateFactory>();
		_ = services.AddTransient<IRoundFactory, RoundFactory>();
		_ = services.AddTransient<IPlayerAggregateFactory, PlayerAggregateFactory>();
		_ = services.AddTransient<IBrandGameProviderAggregateFactory, BrandGameProviderAggregateFactory>();
		_ = services.AddTransient<IGameProviderAggregateFactory, GameProviderAggregateFactory>();
        _ = services.AddTransient<IPlayerBonusAggregateFactory, PlayerBonusAggregateFactory>();
        _ = services.AddTransient<IAggregateExecutionTimeFactory, AggregateExecutionTimeFactory>();

		return services;
	}
}
