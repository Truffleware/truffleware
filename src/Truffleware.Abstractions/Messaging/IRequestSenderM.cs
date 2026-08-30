namespace Truffleware.Abstractions.Messaging;

public abstract record RequestContext<T>(T Request)
{
    public T Request { get; } = Request;
}

public interface IRequestSenderM<in TRequest, TResponse>
{
    Task<TResponse> SendAsync(TRequest request);
}
