namespace Truffleware.Abstractions.Messaging;

/// <summary>
/// Sends a message to respective <see cref="IRequestHandler{TRequest,TResponse}"/>.
/// </summary>
/// <typeparam name="TRequest">Type sent to the handler.</typeparam>
/// <typeparam name="TResponse">Type coming back from the handler.</typeparam>
public interface IRequestSender<in TRequest, TResponse>
{
    Task<TResponse> SendAsync(TRequest request);
}

