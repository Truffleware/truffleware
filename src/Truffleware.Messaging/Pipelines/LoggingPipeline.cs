using Microsoft.Extensions.Logging;

using Truffleware.Abstractions.Messaging;

namespace Truffleware.Messaging.Pipelines;

/// <summary>
/// Logs the request and response. Use with caution on sensitive data.
///
/// Logging is done as a single entry so this should not be used for measuring execution times.
/// </summary>
/// <param name="next">Next pipeline to invoke.</param>
/// <param name="logger">Logger to write into.</param>
/// <typeparam name="TRequest">Incoming type to this pipeline.</typeparam>
/// <typeparam name="TResponse">Outgoing response from this pipeline back to the sender.</typeparam>
public sealed partial class LoggingPipeline<TRequest, TResponse>(
    IRequestPipeline<TRequest, TResponse> next,
    ILogger<LoggingPipeline<TRequest, TResponse>> logger)
    : IRequestPipeline<TRequest, TResponse>
{
    private readonly ILogger<LoggingPipeline<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> InvokeAsync(TRequest request)
    {
        try
        {
            var response = await next.InvokeAsync(request);
            LogRequestResponse(request, response);

            return response;
        }
        catch (Exception ex)
        {
            LogRequestResponse(request, ex);

            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Request {Request} resulted in response: {Response}.")]
    private partial void LogRequestResponse(TRequest request, TResponse response);

    [LoggerMessage(LogLevel.Warning, "Request {Request} failed.")]
    private partial void LogRequestResponse(TRequest request, Exception exception);
}
