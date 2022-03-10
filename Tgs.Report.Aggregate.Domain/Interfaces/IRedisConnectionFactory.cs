using StackExchange.Redis;

namespace Tgs.Report.Aggregate.Domain.Interfaces
{
    public interface IRedisConnectionFactory
    {
        public IDatabase GetDataBase();
    }
}