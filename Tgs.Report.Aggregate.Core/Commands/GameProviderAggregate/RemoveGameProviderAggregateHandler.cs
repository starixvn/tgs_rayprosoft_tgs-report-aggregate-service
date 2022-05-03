using MediatR;
using Tgs.Core.Infrastructures.Utility;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.GameProviderAggregate;

namespace Tgs.Report.Aggregate.Core.Commands.GameProviderAggregate;

public class
	RemoveGameProviderAggregateHandler : IRequestHandler<RemoveGameProviderAggregateRequest,
		MessageResponse>
{
	private readonly IGameProviderAggregateFactory _gameProviderFactory;

	public RemoveGameProviderAggregateHandler(IGameProviderAggregateFactory gameProviderFactory)
	{
		_gameProviderFactory = gameProviderFactory;
	}

	public async Task<MessageResponse> Handle(RemoveGameProviderAggregateRequest request,
		CancellationToken cancellationToken)
	{
		var start = request.StartDate.ToOffset(new(8, 0, 0));

		if (start.Day != 1)
		{
			throw new Exception("The date must be number 1 of each month.");
		}

		var end = DateGenerator.GetFirstDayOfMonth(start.AddMonths(1));

		_ = await _gameProviderFactory.RemoveGameProviderAggregateAsync(request.GameProviderId, start, end);

		return new() { Valid = true };
	}
}
