using System.Transactions;

using MediatR;

using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.RoundAggregate
{
    public class
        ManualFillPlayerBucketMinutelyHandler : IRequestHandler<ManualFillPlayerBucketMinutelyRequest,
            MessageResponse>
    {
        private readonly IPlayerBucketFactory _playerBucketRepository;
        private readonly IRoundFactory _roundService;

        public ManualFillPlayerBucketMinutelyHandler(IRoundFactory roundService,
            IPlayerBucketFactory playerBucketRepository)
        {
            _roundService = roundService;
            _playerBucketRepository = playerBucketRepository;
        }

        public async Task<MessageResponse> Handle(ManualFillPlayerBucketMinutelyRequest request,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.Date) &&
                DateTimeOffset.TryParse($"{request.Date} {request.Hour}:{request.FromMinute} +08:00", out var result) &&
                DateTimeOffset.TryParse($"{request.Date} {request.Hour}:{request.ToMinute} +08:00", out _))
            {
                var start = new DateTimeOffset(result.Year, result.Month, result.Day,
                    result.Hour,
                    request.FromMinute, 0, result.Offset);

                var end = new DateTimeOffset(result.Year, result.Month, result.Day,
                    result.Hour,
                    request.ToMinute, 0, result.Offset).AddMinutes(1);

                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var models = await _roundService.GetMinuteRoundAggregateAsync(start, end);

                if (models.Length > 0 && request.FillPlayerBucket)
                {
                    var playerTurnovers = models.GroupBy(x => x.PlayerCode,
                            (playerCode, list) => new { playerCode, list })
                        .Select(x => new PlayerTurnoverModel
                        {
                            PlayerCode = x.playerCode,
                            Turnover = x.list.Sum(l => l.Turnover)
                        }).ToArray();

                    await _playerBucketRepository.FillInBucketAsync(
                        new() { From = start, To = end, PlayerTurnovers = playerTurnovers });
                }

                scope.Complete();

                return new() { Valid = true };
            }

            return new MessageResponse { Valid = false, Message = "Date is invalid." };
        }
    }
}
