namespace Truffleware.Abstractions.Messaging;

/// <summary>
/// Allows inspection of the incoming request as well as manipulation of the outgoing result.
///
/// Alternatively, this can be used to introduce new side effects.
/// </summary>
/// <typeparam name="TRequest">Incoming type to this pipeline.</typeparam>
/// <typeparam name="TResponse">Outgoing response from his pipeline back to the sender.</typeparam>
public interface IRequestPipeline<in TRequest, TResponse>
{
    Task<TResponse> InvokeAsync(TRequest request);
}
