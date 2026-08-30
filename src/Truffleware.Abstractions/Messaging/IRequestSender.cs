namespace Truffleware.Abstractions.Messaging;

public interface IRequestSender<in TRequest, TResponse>
{
    Task<TResponse> SendAsync(TRequest request);
}

