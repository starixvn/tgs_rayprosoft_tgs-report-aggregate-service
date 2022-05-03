using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Tgs.Report.Aggregate.Client;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ReportAggregateClientExtensions
	{
		public static IServiceCollection AddReportAggregateClient(this IServiceCollection services,
			IConfiguration configuration, string configSectionName = "ReportAggregate")
		{
			_ = services.Configure<ClientSettings>(configuration.GetSection(configSectionName));

			_ = services.AddSingleton(sp => sp.GetRequiredService<IOptions<ClientSettings>>().Value);

			_ = services.AddHttpClient<IReportAggregateClient, ReportAggregateClient>((sp, c) =>
			  {
				  var settings = sp.GetRequiredService<ClientSettings>();
				  c.BaseAddress = new(settings.Url);
			  });

			return services;
		}

		public static IServiceCollection AddReportAggregateClient(this IServiceCollection services,
			Action<ReportAggregateOptions, IServiceProvider> configure)
		{
			_ = services.AddOptions<ReportAggregateOptions>().Configure(configure);

			_ = services.AddHttpClient<IReportAggregateClient, ReportAggregateClient>((sp, c) =>
			{
				var options = sp.GetRequiredService<IOptions<ReportAggregateOptions>>().Value;
				c.BaseAddress = options.BaseUrl;
			});

			return services;
		}
	}
}
