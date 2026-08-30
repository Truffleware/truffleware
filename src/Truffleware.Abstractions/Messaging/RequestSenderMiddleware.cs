namespace Truffleware.Abstractions.Messaging;

public abstract class RequestSenderMiddleware<TContext, TResponse>
{
    protected abstract Task<TResponse> SendAsync(TContext context);
}
