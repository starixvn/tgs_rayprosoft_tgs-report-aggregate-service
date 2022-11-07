using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Aggregate.Core.Workers;
using Tgs.Report.Aggregate.Domain;
using Tgs.Report.Aggregate.Domain.Interfaces;
using Tgs.Report.Aggregate.Domain.Models.PlayerAggregate;
using Tgs.Report.Aggregate.Domain.Shared;

namespace Tgs.Report.Aggregate.Core.Commands.PlayerAggregate;

public class
	ManualCreateUpdatePlayerAggregateHandler : IRequestHandler<ManualCreateUpdatePlayerAggregateModel,
		MessageResponse>
{
	private readonly IPlayerAggregateTaskQueue _taskQueue;
	private readonly ITgsRepository _repository;

	public ManualCreateUpdatePlayerAggregateHandler(
		ITgsRepository repository,
		IPlayerAggregateTaskQueue taskQueue)
	{
		_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		_taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
	}

	public async Task<MessageResponse> Handle(ManualCreateUpdatePlayerAggregateModel request,
		CancellationToken cancellationToken)
	{
		DateTimeOffset reportDate;

		if (!string.IsNullOrEmpty(request.Date) &&
			DateTimeOffset.TryParse($"{request.Date} {request.Hour}:00 +08:00", out var result))
		{
			reportDate = new(result.Year, result.Month, result.Day, 0, 0, 0,
				result.Offset);
		}
		else
		{
			return new MessageResponse { Valid = false, Message = "Date is invalid." };
		}

		var marketsOfDay = await _repository.PlayerAggregates
			.Where(x => x.ReportDate == reportDate.ToUniversalTime())
			.Where(x => x.Hour != request.Hour)
			.Select(x => x.Hour).Distinct().ToArrayAsync(cancellationToken);

		var hourArray = Enumerable.Range(0, result.Hour + 1);

		var doHours = hourArray.Except(marketsOfDay).ToArray();

		if (doHours.Length == 0)
		{
			return new MessageResponse { Valid = false, Message = "Not any hours to generate player aggregate." };
		}

		foreach (var doHour in doHours)
		{
			var start = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
				0, 0, result.Offset);
			var end = new DateTimeOffset(result.Year, result.Month, result.Day, doHour,
				0, 0, result.Offset).AddHours(1);

			await _taskQueue.QueuePlayerAggregateAsync(new(reportDate, start, end));
		}

		return new() { Valid = true };
	}
}
