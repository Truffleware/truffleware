namespace Truffleware.Abstractions.Messaging;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class RequestHandlerAttribute<TRequest, TResponse> : Attribute
{
}
