namespace Truffleware.Abstractions.Messaging;

public interface IRequestSender
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request);
}

public interface IRequestSender<in TRequest, TResponse>
{
    Task<TResponse> SendAsync(TRequest request);
}

