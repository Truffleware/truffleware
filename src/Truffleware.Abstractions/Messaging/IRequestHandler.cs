namespace Truffleware.Abstractions.Messaging;

public interface IRequestHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}
