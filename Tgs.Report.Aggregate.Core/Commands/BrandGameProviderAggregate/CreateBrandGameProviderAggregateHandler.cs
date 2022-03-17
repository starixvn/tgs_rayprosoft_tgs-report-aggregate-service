using MediatR;
using StackExchange.Redis;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.BrandGameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.BrandGameProviderAggregate
{
    public class
        CreateBrandGameProviderAggregateHandler : IRequestHandler<CreateBrandGameProviderAggregateRequest,
            MessageResponse>
    {
        private readonly IBrandGameProviderAggregateFactory _brandGameProviderFactory;
        private readonly IRedisConnectionFactory _redisConnectionFactory;

        public CreateBrandGameProviderAggregateHandler(IBrandGameProviderAggregateFactory brandGameProviderFactory,
            IRedisConnectionFactory redisConnectionFactory)
        {
            _brandGameProviderFactory = brandGameProviderFactory;
            _redisConnectionFactory = redisConnectionFactory;
        }

        public async Task<MessageResponse> Handle(CreateBrandGameProviderAggregateRequest request,
            CancellationToken cancellationToken)
        {
            var db = _redisConnectionFactory.GetDataBase();

            if (await db.SetContainsAsync(request.RedisKey, (RedisValue)request.BrandId.ToString())
                    .ConfigureAwait(false))
            {
                var start = request.StartDate.ToOffset(new(8, 0, 0));
                var end = DateGenerator.GetFirstDayOfMonth(start.AddMonths(1));

                await _brandGameProviderFactory.GenerateBrandGameProviderAggregateAsync(request.BrandId,
                    start.ToUniversalTime(), end.ToUniversalTime(), request.AuditLogId).ConfigureAwait(false);

                await db.SetRemoveAsync(request.RedisKey, request.BrandId.ToString()).ConfigureAwait(false);
            }

            return new() { Valid = true };
        }
    }
}