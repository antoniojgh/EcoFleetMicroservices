using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcoFleet.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Defines a pipeline behavior that logs information about the handling and execution time of requests and responses
/// within the MediatR pipeline.
/// </summary>
/// <remarks>This behavior logs the start and completion of each request, including the request data and the
/// elapsed time for processing. If a request takes longer than 500 milliseconds to process, a warning is logged. This
/// can be used to monitor and diagnose performance issues in the request handling pipeline.</remarks>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the request handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull

{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName} with {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        // Log a warning if the request took longer than 500ms to process
        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long running request: {RequestName} took {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
        }

        _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}