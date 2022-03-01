using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tgs.Report.Aggregate.Domain.Interfaces
{
    public interface ITgsService<T>
    {
        ITgsRepository Repository { get; }

        IQueryable<T> GetQueries(
            Func<IQueryable<T>, IQueryable<T>>? preQuery = null,
            bool tracking = false);

        ValueTask CreateEntitiesAsync(T[] entities);
        ValueTask UpdateEntitiesAsync(T[] entities);
    }
}