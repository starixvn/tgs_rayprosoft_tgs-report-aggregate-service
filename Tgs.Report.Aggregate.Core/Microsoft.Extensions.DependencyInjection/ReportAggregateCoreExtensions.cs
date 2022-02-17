using System.Reflection;

using MediatR;

using Tgs.Report.Aggregate.Core.Factories;
using Tgs.Report.Aggregate.Domain.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportAggregateCoreExtensions
    {
        public static IServiceCollection AddReportAggregateCore(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient<IPlayerBucketFactory, PlayerBucketFactory>();
            services.AddTransient<IPlayerGameAggregateFactory, PlayerGameAggregateFactory>();
            services.AddTransient<IRoundAggregateFactory, RoundAggregateFactory>();
            services.AddTransient<IRoundFactory, RoundFactory>();
            services.AddTransient<IPlayerAggregateFactory, PlayerAggregateFactory>();

            return services;
        }
    }
}
