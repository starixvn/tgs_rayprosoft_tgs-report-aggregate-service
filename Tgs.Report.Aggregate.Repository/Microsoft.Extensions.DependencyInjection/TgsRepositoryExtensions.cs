using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Repository;
using Tgs.Report.Aggregate.Repository.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TgsRepositoryExtensions
    {
        public static IServiceCollection AddTgsRepository(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<TgsRepository>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseLazyLoadingProxies();
            });

            services.AddScoped<ITgsRepository>(c => c.GetRequiredService<TgsRepository>());
            services.AddTransient(typeof(ITgsService<>), typeof(TgsService<>));

            return services;
        }

        public static IServiceCollection AddRedis(
            this IServiceCollection service,
            Action<RedisOptions, IServiceProvider> configure)
        {
            service
                .AddOptions<RedisOptions>()
                .Configure(configure)
                .Services
                .AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();

            return service;
        }
    }
}