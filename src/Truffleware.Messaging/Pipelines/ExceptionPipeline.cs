using Truffleware.Abstractions.Messaging;

namespace Truffleware.Messaging.Pipelines;

/// <summary>
/// Process exceptions within a pipeline.
/// </summary>
/// <typeparam name="TRequest">Incoming type to this pipeline.</typeparam>
/// <typeparam name="TResponse">Outgoing response from this pipeline back to the sender.</typeparam>
public sealed class ExceptionPipeline<TRequest, TResponse>(
    IRequestPipeline<TRequest, TResponse> next)
    : IRequestPipeline<TRequest, TResponse>
{
    private readonly Action<TRequest, Exception>? _action;
    private readonly Func<TRequest, Exception, TResponse>? _fallback;

    /// <summary>
    /// Perform a side effect when an exception occurs.
    ///
    /// The original exception is rethrown after the side effect.
    /// </summary>
    /// <param name="next">Next pipeline to invoke.</param>
    /// <param name="action">Action to perform when an exception occurs.</param>
    public ExceptionPipeline(
        IRequestPipeline<TRequest, TResponse> next,
        Action<TRequest, Exception> action)
        : this(next)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Swallows the original exception and returns from <paramref name="fallback"/> instead.
    /// </summary>
    /// <param name="next">Next pipeline to invoke.</param>
    /// <param name="fallback">Fallback to perform when an exception occurs.</param>
    public ExceptionPipeline(
        IRequestPipeline<TRequest, TResponse> next,
        Func<TRequest, Exception, TResponse> fallback)
        : this(next)
    {
        _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
    }

    public async Task<TResponse> InvokeAsync(TRequest request)
    {
        try
        {
            return await next.InvokeAsync(request);
        }
        catch (Exception ex)
        {
            if (_action is not null)
            {
                _action(request, ex);

                throw;
            }

            if (_fallback is not null)
            {
                return _fallback(request, ex);
            }

            throw new InvalidOperationException("No exception handling strategy was provided.");
        }
    }
}
