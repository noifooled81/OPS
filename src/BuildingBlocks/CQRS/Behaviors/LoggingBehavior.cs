using BuildingBlocks.CQRS.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.CQRS.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICqrsRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
                "Handling request {RequestName}",
                requestName);

        // If this throws, it bypasses the next line and goes to the behavior above (ExceptionHandlingBehavior) which logs the exception. 
        // If it doesn't throw, it continues to the next line and logs completion.
        var response = await next();

        _logger.LogInformation(
            "Completed request {RequestName}",
            requestName);

        return response;
    }
}