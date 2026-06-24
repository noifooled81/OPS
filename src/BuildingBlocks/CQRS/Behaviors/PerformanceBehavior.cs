using System.Diagnostics;
using BuildingBlocks.CQRS.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.CQRS.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
	ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : ICqrsRequest<TResponse>
{
	private const long _slowRequestThresholdMs = 500;

	private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger = logger;

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var stopwatch = Stopwatch.StartNew();

		try
		{
			return await next(cancellationToken);
		}
		finally
		{
			stopwatch.Stop();

			var elapsedMs = stopwatch.ElapsedMilliseconds;
			var requestName = typeof(TRequest).Name;

			if (elapsedMs > _slowRequestThresholdMs)
			{
				_logger.LogWarning(
					"Request {RequestName} completed in {ElapsedMilliseconds} ms (threshold: {ThresholdMilliseconds} ms)",
					requestName,
					elapsedMs,
					_slowRequestThresholdMs);
			}
			else
			{
				_logger.LogInformation(
					"Request {RequestName} completed in {ElapsedMilliseconds} ms",
					requestName,
					elapsedMs);
			}
		}
	}
}
