namespace Truffleware.Abstractions;

public interface IRequestSender<in TRequest, TResponse>
{
    Task<TResponse> SendAsync(TRequest request);
}
