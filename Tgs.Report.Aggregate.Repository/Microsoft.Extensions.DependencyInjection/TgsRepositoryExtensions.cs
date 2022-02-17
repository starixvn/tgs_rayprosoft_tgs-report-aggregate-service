using Microsoft.EntityFrameworkCore;

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
            });

            services.AddScoped<ITgsRepository>(c => c.GetRequiredService<TgsRepository>());

            return services;
        }
    }
}
